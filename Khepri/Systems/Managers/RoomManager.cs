using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
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

        /// <summary> The room a freshly spawned entity (such as the player) is placed into. Currently the first room declared in the world definition. </summary>
        private Room _startingRoom = null!;

        /// <summary> Reverse index from a directly-placed entity to its room, so "which room is X in?" is an O(1) lookup instead of a per-frame containment scan. Maintained by <see cref="PlaceEntity"/> and the initial world build. </summary>
        private readonly Dictionary<Entity, Room> _entityRooms = new Dictionary<Entity, Room>();

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
                LoadPrefabsFrom(path);
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

            _startingRoom = builtRooms.FirstOrDefault()
                ?? throw new InvalidOperationException("The world definition produced no rooms, so no starting room could be determined.");

            IndexRoomContents();

            Logger.LogInformation("World built with {Count} room(s).", _rooms.Count);
        }


        /// <summary> The room into which newly spawned entities (such as the player) are placed at the start of the game. </summary>
        /// <remarks> Currently the first room declared in the world definition; a future world definition could flag an explicit spawn room. </remarks>
        public Room StartingRoom => _startingRoom;


        /// <summary> Places <paramref name="entity"/> directly into <paramref name="room"/>, keeping the reverse index in step so <see cref="GetCurrentRoom"/> stays O(1). </summary>
        /// <remarks> The single choke point for room membership changes; route future movement through here so the index never drifts. </remarks>
        /// <param name="entity"> The entity to place. </param>
        /// <param name="room"> The room to place it in. </param>
        public void PlaceEntity(Entity entity, Room room)
        {
            room.AddEntity(entity);
            _entityRooms[entity] = room;
        }


        /// <summary> Returns every entity placed directly in a room (i.e. every actor candidate), excluding entities nested inside container components. </summary>
        /// <returns> A snapshot of all directly-placed entities across all rooms. </returns>
        public IReadOnlyCollection<Entity> GetAllEntities()
        {
            List<Entity> all = new List<Entity>();

            foreach (Room room in _rooms)
            {
                all.AddRange(room.GetEntities());
            }

            return all;
        }


        /// <summary> Records each room's existing direct entities in the reverse index after the initial world build. </summary>
        private void IndexRoomContents()
        {
            foreach (Room room in _rooms)
            {
                foreach (Entity entity in room.GetEntities())
                {
                    _entityRooms[entity] = room;
                }
            }
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


        /// <summary> Returns the room a directly-placed entity occupies, via the reverse index. </summary>
        /// <remarks> O(1) lookup over directly-placed entities; entities nested inside container components are not tracked here. </remarks>
        /// <param name="entity"> The entity to locate. </param>
        /// <returns> The room the entity is directly placed in. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="entity"/> is not placed in any room. </exception>
        public Room GetCurrentRoom(Entity entity)
        {
            Boolean found = _entityRooms.TryGetValue(entity, out Room? room);

            if (!found)
            {
                throw new InvalidOperationException("The given entity doesn't exist within this plane of reality! This shouldn't be possible.");
            }

            return room!;
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


        /// <summary> Loads every <c>*.tres</c> room prefab in a single directory and registers each by name. </summary>
        /// <param name="directory"> The Godot resource directory path to scan. </param>
        /// <exception cref="InvalidOperationException"> Thrown when the directory cannot be opened, a resource fails to load as a <see cref="RoomPrefab"/>, a prefab name is blank, or two prefabs share a name. </exception>
        private void LoadPrefabsFrom(String directory)
        {
            using DirAccess access = DirAccess.Open(directory);

            if (access is null)
            {
                throw new InvalidOperationException($"Room prefab directory '{directory}' could not be opened (error {DirAccess.GetOpenError()}).");
            }

            foreach (String fileName in access.GetFiles())
            {
                Boolean isResource = fileName.EndsWith(".tres", StringComparison.OrdinalIgnoreCase);

                if (!isResource)
                {
                    continue;
                }

                String     resourcePath = $"{directory}/{fileName}";
                RoomPrefab prefab       = ResourceLoader.Load<RoomPrefab>(resourcePath)
                    ?? throw new InvalidOperationException($"Resource '{resourcePath}' could not be loaded as a RoomPrefab.");

                Register(prefab, resourcePath);
            }
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
