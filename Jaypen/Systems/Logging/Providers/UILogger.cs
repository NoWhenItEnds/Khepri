using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using Microsoft.Extensions.Logging;

namespace Jaypen.Logging.Providers
{
    /// <summary> Appends BBCode-coloured log lines to a <see cref="RichTextLabel"/> node, maintaining a rolling window of at most <see cref="UILoggerProvider.MaxLines"/> entries. </summary>
    public sealed class UILogger : GodotLoggerBase
    {
        /// <summary> Category name prepended to every formatted log line. </summary>
        private readonly String _categoryName;

        /// <summary> The provider that owns the current target label and the configured line limit. Read on every log call so that <see cref="UILoggerProvider.SetTarget"/> takes effect immediately for all existing logger instances. </summary>
        private readonly UILoggerProvider _provider;

        /// <summary> BBCode template applied to levels absent from <see cref="LevelFormats"/>. </summary>
        private const String DefaultFormat = "[color=white]{0}[/color]";

        /// <summary> Maps each MEL <see cref="LogLevel"/> to the BBCode template used when rendering that level, where <c>{0}</c> is the escaped log line. Colours follow a severity brightness ramp shared with <see cref="GodotConsoleLogger"/>: dim for Trace/Debug, neutral for Information, loud colours reserved for Warning and above, with Critical additionally bolded to stand apart from Error. </summary>
        private static readonly IReadOnlyDictionary<LogLevel, String> LevelFormats =
            new Dictionary<LogLevel, String>
            {
                { LogLevel.Trace,       "[color=dim_gray]{0}[/color]"     },
                { LogLevel.Debug,       "[color=gray]{0}[/color]"         },
                { LogLevel.Information, "[color=white]{0}[/color]"        },
                { LogLevel.Warning,     "[color=yellow]{0}[/color]"       },
                { LogLevel.Error,       "[color=red]{0}[/color]"          },
                { LogLevel.Critical,    "[b][color=red]{0}[/color][/b]"   },
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
                String template = LevelFormats.TryGetValue(logLevel, out String? mapped) ? mapped : DefaultFormat;

                // Escape '[' as the BBCode literal '[lb]' so bracketed content in messages (array indexers, user input, exception text) renders as text instead of being parsed as tags — or worse, injecting formatting into the label.
                String payload = $"[{logLevel}] <{_categoryName}> {message}{exceptionSuffix}".Replace("[", "[lb]");
                String formatted = String.Format(CultureInfo.InvariantCulture, template, payload);
                Int32 maxLines = _provider.MaxLines;

                // All variables captured by the lambda are locals; no field references
                // are closed over, so the logger instance does not prevent collection of
                // the label while a deferred call is pending.
                Callable.From(() => AppendDeferred(target, formatted, maxLines)).CallDeferred();
            }
        }


        /// <summary> Appends <paramref name="formatted"/> to <paramref name="target"/>, first pruning the oldest paragraphs while the line limit is exceeded. A single entry may add several paragraphs (e.g. an exception trace), so pruning loops rather than removing a single paragraph per append. </summary>
        /// <param name="target"> The label to mutate; validity is checked before any mutation. </param>
        /// <param name="formatted"> The BBCode-formatted log line to append. </param>
        /// <param name="maxLines"> Maximum paragraph count before the oldest entries are removed. </param>
        private static void AppendDeferred(RichTextLabel target, String formatted, Int32 maxLines)
        {
            if (GodotObject.IsInstanceValid(target))
            {
                // The RemoveParagraph result guards against an infinite loop should the removal ever fail (e.g. an unremovable final paragraph).
                while (target.GetParagraphCount() >= maxLines && target.RemoveParagraph(0))
                {
                }

                target.AppendText(formatted + "\n");
            }
        }
    }
}
