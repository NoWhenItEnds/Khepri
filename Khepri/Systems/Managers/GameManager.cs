using System;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The game world's current timescale. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _timescale = 12f;


        /// <summary> The current, universal time across the entire galaxy. </summary>
        public DateTime UniversalTime { get; private set; } = DateTime.UtcNow;


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            Logger.LogInformation("Hello, World!");
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            UniversalTime += TimeSpan.FromSeconds(delta * _timescale);
        }
    }
}
