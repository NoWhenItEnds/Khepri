using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;
using System;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The current entity controlled by the player. Equivalent to <c>TurnManager.Player.Owner</c>; held here as session state for convenient access. </summary>
        /// <remarks> This should never be null during runtime. </remarks>
        public Entity PlayerEntity = null!;

        /// <summary> The current time within the game world. Advanced by <see cref="AdvanceTime"/> as turns are taken, not by real time. </summary>
        public DateTime GameTime { get; private set; } = DateTime.Now;


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <summary> Bootstraps the session: spawns and places the player, then stands up the turn/control layer and assigns a brain to every actor. </summary>
        /// <remarks> GameManager is the root node, so it readies after <see cref="EntityManager"/> and <see cref="RoomManager"/> — the world already exists at this point. </remarks>
        public override void _Ready()
        {
            Logger.LogInformation("Spawning player...");

            Entity player = EntityManager.Instance!.CreateEntityFromPrefab("goblin");
            SetPlayerEntity(player);

            RoomManager roomManager = RoomManager.Instance!;
            roomManager.PlaceEntity(player, roomManager.StartingRoom);

            // Stand up the turn/control layer. Created at runtime (rather than placed in the scene)
            // so its singleton registers here, after the world has been built.
            TurnManager turnManager = new TurnManager { Name = "Turns" };
            AddChild(turnManager);

            turnManager.SetPlayer(new PlayerController(player));

            // Give every other actor already in the world a default AI brain.
            foreach (Entity entity in roomManager.GetAllEntities())
            {
                Boolean isPlayer = entity.Equals(player);

                if (!isPlayer)
                {
                    turnManager.Register(new AiController(entity));
                }
            }
        }


        /// <summary> Sets the current entity that the player controls. </summary>
        public void SetPlayerEntity(Entity entity)  // TODO - Should the game manager be responsible for the player?
        {
            PlayerEntity = entity;
        }


        /// <summary> Advances the in-game clock by <paramref name="delta"/>. Called by <see cref="TurnManager"/> when a turn completes. </summary>
        /// <param name="delta"> The amount of in-game time that has elapsed. </param>
        public void AdvanceTime(TimeSpan delta)
        {
            GameTime += delta;
        }
    }
}
