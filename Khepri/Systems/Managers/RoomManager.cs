using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Rooms;
using Khepri.Rooms.Prefabs;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing rooms and connections between them. </summary>
    /// <remarks>
    /// Depends on <see cref="EntityManager"/> being initialised before this node's <c>_Ready</c> runs.
    /// In <c>Game.tscn</c> the <c>Entities</c> node precedes the <c>Rooms</c> node as a sibling,
    /// which guarantees Godot's bottom-up <c>_Ready</c> ordering executes EntityManager first.
    /// </remarks>
    public partial class RoomManager : SingletonNode<RoomManager>
    {
        /// <summary> Godot-relative paths to directories containing room prefab JSON files. </summary>
        /// <remarks> Each path is globalised via <see cref="ProjectSettings.GlobalizePath"/> before use. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>
        {
            "res://Khepri/Data/Prefabs/Rooms"
        };

        /// <summary> Godot-relative path to the world definition JSON file that declares rooms and connections. </summary>
        /// <remarks> Globalised via <see cref="ProjectSettings.GlobalizePath"/> before use. </remarks>
        [Export] private String _worldDefinitionPath = "res://Khepri/Data/Worlds/overworld.json";


        /// <summary> All rooms that exist within the game world. </summary>
        private readonly HashSet<Room> _rooms = new HashSet<Room>();

        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<RoomManager>();


        /// <summary> Builds the room catalogue, loads the world definition, constructs all rooms and connections, and populates the internal room set. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="EntityManager"/> has not been initialised, indicating a node ordering problem in <c>Game.tscn</c>. </exception>
        public override void _Ready()
        {
            EntityManager entityManager = EntityManager.Instance
                ?? throw new InvalidOperationException("RoomManager._Ready requires EntityManager to be initialised first. Ensure the Entities node precedes the Rooms node in Game.tscn.");

            Logger.LogInformation("Building room catalogue...");

            FeatureRegistry  registry  = new FeatureRegistry();
            FeatureDiscovery.RegisterAll(registry);

            RoomCatalogue catalogue = new RoomCatalogue(registry);

            foreach (String path in _prefabPaths)
            {
                catalogue.LoadDirectory(ProjectSettings.GlobalizePath(path));
            }

            Logger.LogInformation("Loading world definition from '{Path}'...", _worldDefinitionPath);

            WorldDefinition worldDefinition = new WorldDefinitionLoader().Load(
                ProjectSettings.GlobalizePath(_worldDefinitionPath));

            Logger.LogInformation("Building world...");

            IReadOnlyCollection<Room> builtRooms = new WorldBuilder(
                catalogue,
                entityManager.CreateEntityFromPrefab).Build(worldDefinition);

            foreach (Room room in builtRooms)
            {
                _rooms.Add(room);
            }

            Logger.LogInformation("World built with {Count} room(s).", _rooms.Count);
        }


        /// <summary> Creates a bidirectional <see cref="Connection"/> between two existing rooms and registers it on both endpoints. </summary>
        /// <remarks> Use at runtime to wire user-created passages not declared in the world definition file. </remarks>
        /// <param name="roomA"> The first room endpoint; must already be tracked by this manager. </param>
        /// <param name="roomB"> The second room endpoint; must already be tracked by this manager and differ from <paramref name="roomA"/>. </param>
        /// <exception cref="ArgumentException"> Thrown when either room is not tracked by this manager, or when both refer to the same instance. </exception>
        public void AddConnection(Room roomA, Room roomB)
        {
            Boolean roomAKnown = _rooms.Contains(roomA);

            if (!roomAKnown)
            {
                throw new ArgumentException("The room is not tracked by this RoomManager.", nameof(roomA));
            }

            Boolean roomBKnown = _rooms.Contains(roomB);

            if (!roomBKnown)
            {
                throw new ArgumentException("The room is not tracked by this RoomManager.", nameof(roomB));
            }

            Connection connection = new Connection(roomA, roomB);
            roomA.AddConnection(connection);
            roomB.AddConnection(connection);
        }


        /// <summary> Returns the room that contains the given entity, searching the full <see cref="IEntityContainer"/> hierarchy. </summary>
        /// <param name="entity"> The entity to locate. </param>
        /// <returns> The room whose containment tree includes <paramref name="entity"/> at any depth. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="entity"/> cannot be found in any room. </exception>
        public Room GetCurrentRoom(Entity entity)
        {
            Room? result = _rooms.FirstOrDefault(room => ((IEntityContainer)room).Contains(entity));

            if (result == null)
            {
                throw new InvalidOperationException("The given entity doesn't exist within this plane of reality! This shouldn't be possible.");
            }

            return result;
        }
    }
}
