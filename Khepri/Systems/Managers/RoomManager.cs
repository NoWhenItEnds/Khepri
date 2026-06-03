using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.Extensions;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Rooms;
using Khepri.Rooms.Definitions;
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
        /// <summary> The Godot resource directories to scan for room prefab (<c>*.tres</c>) definitions. </summary>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>
        {
            "res://Khepri/Data/Prefabs/Rooms"
        };

        /// <summary> Godot resource path to the world definition JSON file that declares rooms and connections. </summary>
        [Export] private String _worldDefinitionPath = "res://Khepri/Data/Worlds/overworld.json";


        /// <summary> All loaded room prefabs, keyed by their <see cref="RoomPrefab.Name"/>. </summary>
        private readonly Dictionary<String, RoomPrefab> _prefabsByName = new Dictionary<String, RoomPrefab>();

        /// <summary> All rooms that exist within the game world. </summary>
        private readonly HashSet<Room> _rooms = new HashSet<Room>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<RoomManager>();


        /// <summary> Loads the room prefabs, loads the world definition, constructs all rooms and connections, and populates the internal room set. </summary>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="EntityManager"/> has not been initialised, indicating a node ordering problem in <c>Game.tscn</c>. </exception>
        public override void _Ready()
        {
            EntityManager entityManager = EntityManager.Instance
                ?? throw new InvalidOperationException("RoomManager._Ready requires EntityManager to be initialised first. Ensure the Entities node precedes the Rooms node in Game.tscn.");

            Logger.LogInformation("Loading room prefabs...");

            foreach (String path in _prefabPaths)
            {
                foreach (RoomPrefab prefab in ResourceExtensions.GetResources<RoomPrefab>(path))
                {
                    Register(prefab, prefab.ResourcePath);
                }
            }

            Logger.LogInformation("Loading world definition from '{Path}'...", _worldDefinitionPath);

            WorldDefinition worldDefinition = new WorldDefinitionLoader().Load(
                ProjectSettings.GlobalizePath(_worldDefinitionPath));

            Logger.LogInformation("Building world...");

            IReadOnlyCollection<Room> builtRooms = new WorldBuilder(
                CreateRoomFromPrefab,
                entityManager.CreateEntityFromPrefab).Build(worldDefinition);

            foreach (Room room in builtRooms)
            {
                _rooms.Add(room);
            }

            Logger.LogInformation("World built with {Count} room(s).", _rooms.Count);
        }


        /// <summary> Creates a bidirectional <see cref="Connection"/> between two existing rooms and registers it on both endpoints. </summary>
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


        /// <summary> Get all the rooms in the game world. </summary>
        /// <returns> An immutable list containing all the discovered rooms in the game world.</returns>
        public IReadOnlyCollection<Room> GetRooms() => _rooms.ToArray();


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


        /// <summary> Builds a new <see cref="Room"/> from the named prefab, or returns <c>null</c> when no such prefab is loaded. </summary>
        /// <remarks> Passed to <see cref="WorldBuilder"/> as its room-factory seam. </remarks>
        /// <param name="prefabName"> The room prefab name to resolve. </param>
        /// <returns> A freshly built room, or <c>null</c> when the name is unknown. </returns>
        private Room? CreateRoomFromPrefab(String prefabName)
        {
            Boolean found = _prefabsByName.TryGetValue(prefabName, out RoomPrefab? prefab);
            return found ? prefab!.Instantiate() : null;
        }


        /// <summary> Registers a single room prefab by its name, rejecting blanks and duplicates. </summary>
        /// <param name="prefab"> The loaded prefab to register. </param>
        /// <param name="resourcePath"> The resource path, used only to enrich error messages. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="RoomPrefab.Name"/> is blank or already registered. </exception>
        private void Register(RoomPrefab prefab, String resourcePath)
        {
            if (String.IsNullOrWhiteSpace(prefab.Name))
            {
                throw new InvalidOperationException($"Room prefab '{resourcePath}' has a blank Name.");
            }

            Boolean duplicate = _prefabsByName.ContainsKey(prefab.Name);

            if (duplicate)
            {
                throw new InvalidOperationException($"Duplicate room prefab name '{prefab.Name}' (from '{resourcePath}').");
            }

            _prefabsByName[prefab.Name] = prefab;
        }
    }
}
