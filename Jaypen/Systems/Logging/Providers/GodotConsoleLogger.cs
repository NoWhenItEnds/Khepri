using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Extensions.Logging;

namespace Jaypen.Logging.Providers
{
    /// <summary> Writes log entries to the Godot editor console, mapping each MEL <see cref="LogLevel"/> to the appropriate Godot output method and BBCode colour. </summary>
    /// <remarks> Dispatch is driven by a static lookup table so new levels require only a new table entry; no conditional logic needs modification. Colours follow a severity brightness ramp shared with <see cref="UILogger"/> — dim for Trace/Debug, neutral for Information — while Warning and above route to <see cref="GD.PushWarning(String)"/>/<see cref="GD.PushError(String)"/> for the editor's engine-styled panels. </remarks>
    public sealed class GodotConsoleLogger : GodotLoggerBase
    {
        /// <summary> Category name (typically the fully-qualified type name) prepended to every log line. </summary>
        private readonly String _categoryName;

        /// <summary> Maps each <see cref="LogLevel"/> to the Godot output action for that severity. Levels absent from the map produce no output (<see cref="LogLevel.None"/>). </summary>
        /// <remarks> Only the <see cref="GD.PrintRich(Godot.Variant[])"/> sinks escape their input: they parse BBCode, whereas <c>PushWarning</c>/<c>PushError</c> are plain text and would display the escape sequence literally. </remarks>
        private static readonly IReadOnlyDictionary<LogLevel, Action<String>> Sinks =
            new Dictionary<LogLevel, Action<String>>
            {
                { LogLevel.Trace,       s => GD.PrintRich($"[color=dim_gray]{EscapeBBCode(s)}[/color]") },
                { LogLevel.Debug,       s => GD.PrintRich($"[color=gray]{EscapeBBCode(s)}[/color]") },
                { LogLevel.Information, s => GD.PrintRich($"[color=white]{EscapeBBCode(s)}[/color]") },
                { LogLevel.Warning,     s => GD.PushWarning(s) },
                { LogLevel.Error,       s => GD.PushError(s) },
                { LogLevel.Critical,    s => GD.PushError(s) },
            };


        /// <summary> Initialises the logger with the owning category name. </summary>
        /// <param name="categoryName"> Category identifier prepended to every formatted message. </param>
        public GodotConsoleLogger(String categoryName)
        {
            _categoryName = categoryName;
        }


        /// <inheritdoc/>
        public override void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, String> formatter)
        {
            String message = formatter(state, exception);
            String exceptionSuffix = exception != null ? $"\n{exception}" : String.Empty;
            String formatted = $"[{logLevel}] <{_categoryName}> {message}{exceptionSuffix}";

            if (Sinks.TryGetValue(logLevel, out Action<String>? sink))
            {
                sink(formatted);
            }
        }


        /// <summary> Escapes <c>[</c> as the BBCode literal <c>[lb]</c> so bracketed content in messages (array indexers, exception text) renders as text instead of being parsed as tags. </summary>
        /// <param name="text"> The raw log line to escape. </param>
        /// <returns> The escaped text, safe to embed inside BBCode markup. </returns>
        private static String EscapeBBCode(String text) => text.Replace("[", "[lb]");
    }
}
