using Godot;
using Jaypen.Utilities.Extensions;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Entities.Controllers;
using Khepri.Rooms;
using Khepri.Worlds;
using Khepri.Worlds.Definitions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    /// <remarks> Its <c>_Ready</c> is the single bootstrap point: it loads the world definitions, runs world construction. </remarks>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The current entity controlled by the player. Equivalent to <c>TurnManager.Player.Owner</c>; held here as session state for convenient access. </summary>
        /// <remarks> This should never be null during runtime. </remarks>
        public Entity PlayerEntity = null!;

        /// <summary> The current time within the game world. Advanced by <see cref="AdvanceTime"/> as turns are taken, not by real time. </summary>
        public DateTime GameTime { get; private set; } = DateTime.Now;

        /// <summary> The Godot resource directories to scan for <see cref="WorldDefinition"/> (<c>*.tres</c>) definitions. </summary>
        /// <remarks> Paths are Godot resource paths (e.g. <c>res://Khepri/Data/Worlds</c>). Every <c>.tres</c> in each directory is loaded as a <see cref="WorldDefinition"/> and built into the shared room set. Use the <c>res://</c> scheme — do not globalise these paths. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _worldDefinitionPaths = new Godot.Collections.Array<String>
        {
            "res://Khepri/Data/Worlds"
        };


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            RoomManager roomManager = RoomManager.Instance
                ?? throw new InvalidOperationException("GameManager._Ready requires RoomManager to be initialised first. Ensure the Rooms node precedes the GameManager root node in Game.tscn.");

            EntityManager entityManager = EntityManager.Instance
                ?? throw new InvalidOperationException("GameManager._Ready requires EntityManager to be initialised first. Ensure the Entities node precedes the GameManager root node in Game.tscn.");

            List<WorldDefinition> worldDefinitions = new List<WorldDefinition>();

            foreach (String path in _worldDefinitionPaths)
            {
                Logger.LogInformation("Loading world definitions from '{Path}'...", path);
                worldDefinitions.AddRange(ResourceExtensions.GetResources<WorldDefinition>(path));
            }

            if (worldDefinitions.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No world definitions were found in any configured directory ({String.Join(", ", _worldDefinitionPaths)}). Ensure at least one valid WorldDefinition resource exists.");
            }

            Logger.LogInformation("Building {WorldCount} world(s)...", worldDefinitions.Count);

            IReadOnlyCollection<Room> builtRooms =
                new WorldBuilder(roomManager.CreateRoom, entityManager.CreateEntity, roomManager.AddConnection)
                    .Build(worldDefinitions);

            Logger.LogInformation("World built with {RoomCount} room(s).", builtRooms.Count);

            Logger.LogInformation("Spawning player...");

            Entity player = entityManager.CreateEntity("goblin");
            PlayerController playerController = new PlayerController(player);
            entityManager.SetController(player, playerController);
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


        // TODO - Add turns that advance time.
    }
}
