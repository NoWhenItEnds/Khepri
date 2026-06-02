using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Microsoft.Extensions.Logging;
using System;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The current time within the game world. </summary>
        public DateTime GameTime { get; private set; } = DateTime.Now;


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
            GameTime = DateTime.Now;
        }
    }
}
