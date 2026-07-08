using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Extensions.Logging;

namespace Jaypen.Logging.Providers
{
    /// <summary> MEL provider that writes log entries to a file, keeping the handle open for the lifetime of the provider so that each entry is appended efficiently. </summary>
    /// <remarks> Each provider instance owns one file — callers embed a session timestamp in the name (see <c>Log.BuildLogFilePath</c>) so every session writes a fresh file. On construction, older <c>.log</c> files in the same directory are pruned so at most <see cref="MaxRetainedLogs"/> remain. Flushes after every write so logs survive a crash mid-session; the per-write flush is a deliberate trade-off against throughput. Do not remove it without replacing the crash-safety guarantee. </remarks>
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        /// <summary> Maximum number of <c>.log</c> files retained in the log directory, including the current session's file; the oldest beyond this count are deleted on construction. </summary>
        private const Int32 MaxRetainedLogs = 10;

        /// <summary> Object used to serialise file writes across threads. </summary>
        private readonly Object _lock = new Object();

        /// <summary> Open file handle; null when the file could not be opened. </summary>
        /// <remarks> Writes produce a single diagnostic error on the first attempt (see <see cref="_firstWriteWarned"/>) and are silent no-ops thereafter. </remarks>
        private FileAccess? _file;

        /// <summary> Guards against double-dispose, and suppresses the missing-file diagnostic for writes that arrive after shutdown. </summary>
        private Boolean _disposed;

        /// <summary> Ensures the "file logging disabled" warning is emitted at most once, on the first write attempt after a failed open, rather than on every subsequent write. </summary>
        private Boolean _firstWriteWarned;

        /// <summary> Virtual path to the log file, supplied by the caller. </summary>
        private readonly String _filePath;

        /// <summary> Delegate stored so the <see cref="AppDomain.ProcessExit"/> subscription can be removed precisely in <see cref="Dispose"/> without allocating a new closure. </summary>
        private readonly EventHandler _processExitHandler;


        /// <summary> Creates the log directory if absent, opens <paramref name="filePath"/> for writing, then prunes old log files beyond <see cref="MaxRetainedLogs"/>. </summary>
        /// <param name="filePath"> Godot virtual path (e.g. <c>user://logs/Khepri_2026-07-08_14-23-05.log</c>) of the target log file. </param>
        public FileLoggerProvider(String filePath)
        {
            _filePath = filePath;
            _processExitHandler = OnProcessExit;
            AppDomain.CurrentDomain.ProcessExit += _processExitHandler;
            OpenFile();
        }


        /// <inheritdoc/>
        public ILogger CreateLogger(String categoryName) => new FileLogger(categoryName, this);


        /// <summary> Appends <paramref name="line"/> to the open file and flushes the buffer so the entry survives a crash. Thread-safe via an internal lock. </summary>
        /// <param name="line"> The fully-formatted log line including trailing newline. </param>
        public void WriteLine(String line)
        {
            lock (_lock)
            {
                if (_file != null)
                {
                    _file.StoreString(line);
                    _file.Flush();
                }
                else if (!_disposed && !_firstWriteWarned)
                {
                    GD.PushError("[FileLoggerProvider] File logging is disabled because the log file could not be opened — see earlier error for details.");
                    _firstWriteWarned = true;
                }
            }
        }


        /// <summary> Flushes and closes the file handle. Safe to call multiple times. </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                AppDomain.CurrentDomain.ProcessExit -= _processExitHandler;

                lock (_lock)
                {
                    if (_file != null)
                    {
                        _file.Flush();
                        _file.Close();
                        _file = null;
                    }

                    _disposed = true;
                }
            }
        }


        /// <summary> Creates the log file's parent directory if absent, opens the file in <c>Write</c> mode, then prunes stale logs via <see cref="PruneOldLogs"/>. </summary>
        private void OpenFile()
        {
            String logDirectory = _filePath.GetBaseDir();
            Error mkdirError = DirAccess.MakeDirRecursiveAbsolute(logDirectory);

            // Godot returns Error.AlreadyExists when the directory is already present — this is not a failure; the directory is ready to use.
            Boolean directoryReady = (mkdirError == Error.Ok || mkdirError == Error.AlreadyExists);

            if (!directoryReady)
            {
                GD.PushError($"[FileLoggerProvider] Could not create log directory '{logDirectory}': {mkdirError}");
            }
            else
            {
                FileAccess? handle = FileAccess.Open(_filePath, FileAccess.ModeFlags.Write);

                if (handle == null)
                {
                    Error openError = FileAccess.GetOpenError();
                    GD.PushError($"[FileLoggerProvider] Could not open '{_filePath}' for writing: {openError}");
                }
                else
                {
                    _file = handle;
                    PruneOldLogs(logDirectory);
                }
            }
        }


        /// <summary> Deletes the oldest <c>.log</c> files in <paramref name="logDirectory"/> until at most <see cref="MaxRetainedLogs"/> remain, so per-session files cannot accumulate without bound. </summary>
        /// <param name="logDirectory"> The directory whose log files are pruned. </param>
        /// <remarks> Age is determined by ordinal filename order, which is chronological because the session timestamp format is fixed-width. The current session's file is never a candidate: it sorts newest, and pruning stops well before reaching it. </remarks>
        private void PruneOldLogs(String logDirectory)
        {
            using DirAccess? dir = DirAccess.Open(logDirectory);

            if (dir != null)
            {
                List<String> logFiles = dir.GetFiles()
                    .Where(fileName => fileName.EndsWith(".log", StringComparison.Ordinal))
                    .OrderBy(fileName => fileName, StringComparer.Ordinal)
                    .ToList();

                Int32 excessCount = logFiles.Count - MaxRetainedLogs;
                foreach (String fileName in logFiles.Take(Math.Max(excessCount, 0)))
                {
                    String stalePath = $"{logDirectory}/{fileName}";
                    Error removeError = DirAccess.RemoveAbsolute(stalePath);

                    if (removeError != Error.Ok)
                    {
                        GD.PushWarning($"[FileLoggerProvider] Could not delete stale log '{stalePath}': {removeError}");
                    }
                }
            }
        }


        /// <summary> Invoked by the CLR runtime on normal process exit; calls <see cref="Dispose"/> to ensure the file handle is flushed and closed. </summary>
        /// <param name="sender"> The <see cref="AppDomain"/> raising the event; not used. </param>
        /// <param name="e"> Event arguments; not used. </param>
        private void OnProcessExit(Object? sender, EventArgs e)
        {
            Dispose();
        }
    }
}
