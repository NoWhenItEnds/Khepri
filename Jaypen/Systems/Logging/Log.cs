using System;
using System.Threading;
using Godot;
using Microsoft.Extensions.Logging;
using Jaypen.Utilities.Logging.Providers;

namespace Jaypen.Utilities.Logging
{
    /// <summary> Stateless entry point for obtaining <see cref="ILogger"/> instances throughout the application. Owns a single <see cref="ILoggerFactory"/> that is initialised lazily on first use and lives for the process lifetime. </summary>
    public static class Log
    {
        /// <summary> Process-lifetime factory; initialised exactly once on first access. <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> ensures a single initialisation attempt across all threads. </summary>
        private static readonly Lazy<ILoggerFactory> _factory =
            new Lazy<ILoggerFactory>(BuildFactory, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary> The UI provider installed at factory construction time; its target is set via <see cref="SetUiTarget"/> once the scene node is available. </summary>
        private static readonly UILoggerProvider _uiProvider = new UILoggerProvider();


        /// <summary> Returns an <see cref="ILogger"/> scoped to <typeparamref name="T"/>, using the fully-qualified type name as the category. </summary>
        /// <typeparam name="T"> Type whose full name is used as the log category. </typeparam>
        /// <returns> An <see cref="ILogger"/> for the specified type. </returns>
        public static ILogger For<T>() => _factory.Value.CreateLogger<T>();


        /// <summary> Returns an <see cref="ILogger"/> scoped to <paramref name="categoryName"/>, for cases where a type reference is not available (e.g. scripts, utilities). </summary>
        /// <param name="categoryName"> Arbitrary category string prepended to every log entry. </param>
        /// <returns> An <see cref="ILogger"/> for the specified category name. </returns>
        public static ILogger For(String categoryName) => _factory.Value.CreateLogger(categoryName);


        /// <summary> Wires the <see cref="UILoggerProvider"/> to <paramref name="target"/> so that all current and future logger instances begin writing to the UI label. </summary>
        /// <param name="target"> The <see cref="RichTextLabel"/> node that should receive log output; must have BBCode enabled. </param>
        /// <remarks> The <see cref="UILoggerProvider"/> is installed in the factory at construction time, so all loggers — including those resolved before this call — will write to <paramref name="target"/> immediately after this method returns. </remarks>
        public static void SetUiTarget(RichTextLabel target)
        {
            _uiProvider.SetTarget(target);
        }


        /// <summary> Constructs and configures the <see cref="ILoggerFactory"/> with the default providers. </summary>
        /// <returns> A fully configured <see cref="ILoggerFactory"/>. </returns>
        /// <remarks> Minimum level is <see cref="LogLevel.Debug"/> in DEBUG builds and <see cref="LogLevel.Information"/> in all other configurations. </remarks>
        private static ILoggerFactory BuildFactory()
        {
            return LoggerFactory.Create(builder =>
            {
#if DEBUG
                builder.SetMinimumLevel(LogLevel.Debug);
#else
                builder.SetMinimumLevel(LogLevel.Information);
#endif
                builder.AddProvider(new GodotConsoleLoggerProvider());
                builder.AddProvider(new FileLoggerProvider("user://logs/Jaypen.log"));
                builder.AddProvider(_uiProvider);
            });
        }
    }
}
