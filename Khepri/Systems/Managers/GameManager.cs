using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Microsoft.Extensions.Logging;
using System;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The current entity controlled by the player. </summary>
        /// <remarks> This should never be null during runtime. </remarks>
        public Entity PlayerEntity = null!;

        /// <summary> The current time within the game world. </summary>
        public DateTime GameTime { get; private set; } = DateTime.Now;


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            Logger.LogInformation("Spawning player...");
            SetPlayerEntity(EntityManager.Instance!.CreateEntityFromPrefab("goblin"));
        }


        /// <summary> Sets the current entity that the player controls. </summary>
        public void SetPlayerEntity(Entity entity)  // TODO - Should the game manager be responsible for the player?
        {
            PlayerEntity = entity;
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            GameTime = DateTime.Now;
        }
    }
}
