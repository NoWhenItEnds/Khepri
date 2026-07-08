using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using FileAccess = Godot.FileAccess;

namespace Jaypen.Extensions
{
    /// <summary> A helper class for working with files and filepaths. </summary>
    public static class FileExtensions
    {
        /// <summary> Reads the entire file at <paramref name="filePath"/> as text, converting Godot's silent-failure idiom (empty string plus a pending open error) into a proper exception. </summary>
        /// <param name="filePath"> The Godot virtual path (e.g. <c>res://...</c> or <c>user://...</c>) of the file to read. </param>
        /// <returns> The file's full contents. </returns>
        /// <exception cref="FileNotFoundException"> Thrown when Godot cannot open the file at <paramref name="filePath"/>. </exception>
        public static String ReadAllText(String filePath)
        {
            String contents = FileAccess.GetFileAsString(filePath);
            Error openError = FileAccess.GetOpenError();

            if (openError != Error.Ok)
            {
                throw new FileNotFoundException($"Could not open '{filePath}': Godot error {openError}.", filePath);
            }

            return contents;
        }


        /// <summary> Search the given directory, recursively, for files. </summary>
        /// <param name="directoryPath"> The Godot filepath at which to begin the search. </param>
        /// <param name="extensions"> Set of file extensions (including the leading dot, e.g. <c>".tres"</c>) to accept. Pass <see langword="null"/> to accept all extensions. </param>
        /// <returns> The filepaths of all matched files in breadth-first order. </returns>
        /// <exception cref="DirectoryNotFoundException"> Thrown when <paramref name="directoryPath"/> cannot be opened by Godot. </exception>
        public static String[] GetFilepaths(String directoryPath, HashSet<String>? extensions = null)
        {
            using DirAccess? rootAccess = DirAccess.Open(directoryPath);
            if (rootAccess == null)
            {
                throw new DirectoryNotFoundException($"The '{directoryPath}' directory does not exist.");
            }

            List<String> results = new List<String>();
            HashSet<String> visited = new HashSet<String>(StringComparer.Ordinal);
            Queue<String> pending = new Queue<String>();
            pending.Enqueue(directoryPath);

            while (pending.Count > 0)
            {
                String currentDir = pending.Dequeue();

                // Normalise the virtual path to its absolute form so the same directory reached
                // via different virtual spellings (e.g. res:// vs an absolute path) is detected
                // as already-visited. Symlinks are NOT resolved, so a symlink cycle expressed
                // through distinct absolute paths would not be caught by this guard.
                String normalisedDir = ProjectSettings.GlobalizePath(currentDir);
                Boolean alreadyVisited = !visited.Add(normalisedDir);

                if (!alreadyVisited)
                {
                    using DirAccess? dir = DirAccess.Open(currentDir);
                    if (dir != null)
                    {
                        dir.ListDirBegin();
                        String entry = dir.GetNext();
                        while (!String.IsNullOrEmpty(entry))
                        {
                            String entryPath = currentDir + "/" + entry;
                            if (dir.CurrentIsDir())
                            {
                                pending.Enqueue(entryPath);
                            }
                            else
                            {
                                Boolean accepted = extensions == null
                                    || extensions.Contains(Path.GetExtension(entryPath));
                                if (accepted)
                                {
                                    results.Add(entryPath);
                                }
                            }

                            entry = dir.GetNext();
                        }

                        dir.ListDirEnd();
                    }
                }
            }

            return results.ToArray();
        }
    }
}
