using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Extensions.Logging;

namespace Jaypen.Utilities.Logging.Providers
{
    /// <summary> Appends BBCode-coloured log lines to a <see cref="RichTextLabel"/> node, maintaining a rolling window of at most <see cref="UILoggerProvider.MaxLines"/> entries. /// </summary>
    public sealed class UILogger : GodotLoggerBase
    {
        /// <summary> Category name prepended to every formatted log line. </summary>
        private readonly String _categoryName;

        /// <summary> The provider that owns the current target label and the configured line limit. Read on every log call so that <see cref="UILoggerProvider.SetTarget"/> takes effect immediately for all existing logger instances. </summary>
        private readonly UILoggerProvider _provider;

        /// <summary> Maps each MEL <see cref="LogLevel"/> to the BBCode colour name used when rendering that level. </summary>
        private static readonly IReadOnlyDictionary<LogLevel, String> LevelColours =
            new Dictionary<LogLevel, String>
            {
                { LogLevel.Trace,       "white"  },
                { LogLevel.Debug,       "white"  },
                { LogLevel.Information, "green"  },
                { LogLevel.Warning,     "yellow" },
                { LogLevel.Error,       "red"    },
                { LogLevel.Critical,    "red"    },
            };


        /// <summary> Initialises the logger with its category name and the owning provider. </summary>
        /// <param name="categoryName"> Category identifier prepended to every formatted message. </param>
        /// <param name="provider"> The provider that supplies the current target label and line limit. </param>
        public UILogger(String categoryName, UILoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }


        /// <inheritdoc/>
        public override void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, String> formatter)
        {
            RichTextLabel? target = _provider.Target;
            if (target != null)
            {
                String message = formatter(state, exception);
                String exceptionSuffix = exception != null ? $"\n{exception}" : String.Empty;
                String colour = LevelColours.TryGetValue(logLevel, out String? mapped) ? mapped : "white";
                String formatted = $"[color={colour}][{logLevel}] <{_categoryName}> {message}{exceptionSuffix}[/color]";
                Int32 maxLines = _provider.MaxLines;

                // All variables captured by the lambda are locals; no field references
                // are closed over, so the logger instance does not prevent collection of
                // the label while a deferred call is pending.
                Callable.From(() => AppendDeferred(target, formatted, maxLines)).CallDeferred();
            }
        }


        /// <summary> Appends <paramref name="formatted"/> to <paramref name="target"/>, first pruning the oldest paragraph when the line limit is exceeded. </summary>
        /// <param name="target"> The label to mutate; validity is checked before any mutation. </param>
        /// <param name="formatted"> The BBCode-formatted log line to append. </param>
        /// <param name="maxLines"> Maximum paragraph count before the oldest entry is removed. </param>
        private static void AppendDeferred(RichTextLabel target, String formatted, Int32 maxLines)
        {
            if (GodotObject.IsInstanceValid(target))
            {
                if (target.GetParagraphCount() >= maxLines)
                {
                    target.RemoveParagraph(0);
                }

                target.AppendText(formatted + "\n");
            }
        }
    }
}
