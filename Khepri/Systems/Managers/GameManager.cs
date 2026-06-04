using Godot;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    /// <remarks> Its <c>_Ready</c> is the single bootstrap point: it loads the world definition, runs world construction. </remarks>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The current entity controlled by the player. Equivalent to <c>TurnManager.Player.Owner</c>; held here as session state for convenient access. </summary>
        /// <remarks> This should never be null during runtime. </remarks>
        public Entity PlayerEntity = null!;

        /// <summary> The current time within the game world. Advanced by <see cref="AdvanceTime"/> as turns are taken, not by real time. </summary>
        public DateTime GameTime { get; private set; } = DateTime.Now;

        /// <summary> Godot resource path to the <c>WorldDefinition</c> <c>.tres</c> file that declares rooms and connections. Use the <c>res://</c> scheme — do not globalise this path. </summary>
        [ExportGroup("Settings")]
        [Export] private String _worldDefinitionPath = "res://Khepri/Data/Worlds/Overworld.tres";


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <summary> Bootstraps the session: loads the world definition, builds all rooms and connections, then spawns and places the player. </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="RoomManager"/> or <see cref="EntityManager"/> has not been initialised (node ordering problem in <c>Game.tscn</c>),
        /// or when the world definition resource cannot be loaded from <see cref="_worldDefinitionPath"/>.
        /// </exception>
        public override void _Ready()
        {
            RoomManager roomManager = RoomManager.Instance
                ?? throw new InvalidOperationException("GameManager._Ready requires RoomManager to be initialised first. Ensure the Rooms node precedes the GameManager root node in Game.tscn.");

            EntityManager entityManager = EntityManager.Instance
                ?? throw new InvalidOperationException("GameManager._Ready requires EntityManager to be initialised first. Ensure the Entities node precedes the GameManager root node in Game.tscn.");

            Logger.LogInformation("Loading world definition from '{Path}'...", _worldDefinitionPath);

            WorldDefinition? worldDefinition = ResourceLoader.Load<WorldDefinition>(_worldDefinitionPath);

            if (worldDefinition is null)
            {
                throw new InvalidOperationException(
                    $"World definition resource could not be loaded from '{_worldDefinitionPath}'. Ensure the file exists and is a valid WorldDefinition resource.");
            }

            Logger.LogInformation("Building world...");

            IReadOnlyCollection<Room> builtRooms = new WorldBuilder(
                roomManager.CreateRoom,
                entityManager.CreateEntity,
                roomManager.AddConnection).Build(worldDefinition);

            Logger.LogInformation("World built with {Count} room(s).", builtRooms.Count);

            Logger.LogInformation("Spawning player...");

            Entity player = entityManager.CreateEntity("goblin");
            SetPlayerEntity(player);

            Room startingRoom = roomManager.GetRooms().First();
            startingRoom.AddEntity(player, RoomPosition.Center);
        }


        /// <summary> Sets the current entity that the player controls. </summary>
        /// <param name="entity"> The entity to assign as the player-controlled entity. </param>
        public void SetPlayerEntity(Entity entity)  // TODO - Should the game manager be responsible for the player?
        {
            PlayerEntity = entity;
        }
    }
}
