using Khepri.Entities;
using System;
using System.Collections.Generic;

namespace Khepri.Rooms
{
    /// <summary> Constructs a live, connected world from a parsed <see cref="WorldDefinition"/> in two deterministic passes: first instantiate and populate all rooms, then wire their connections. </summary>
    /// <remarks>
    /// Godot-free and decoupled from the managers. Both construction responsibilities are injected as delegates: <c>roomFactory</c> turns a room prefab name into a fresh <see cref="Room"/>, and <c>entitySpawner</c> turns an entity prefab name into a fresh <see cref="Entity"/>. These are the only seams between this POCO layer and the Godot layer above it.
    /// Pass 1 — for each <see cref="RoomInstanceSpec"/>, build the room via <c>roomFactory</c>, record the instance id-to-<see cref="Room"/> mapping, then spawn each declared entity into the room.
    /// Pass 2 — for each <see cref="ConnectionSpec"/>, look up both rooms by instance id, construct a <see cref="Connection"/>, and register it on both room endpoints.
    /// </remarks>
    public sealed class WorldBuilder
    {
        /// <summary> Resolves a room prefab name to a freshly built <see cref="Room"/>, or returns <c>null</c> when the name is unknown. </summary>
        private readonly Func<String, Room?> _roomFactory;

        /// <summary> Maps an entity prefab name to a freshly spawned <see cref="Entity"/>; provided by the Godot layer so this class stays Godot-free. </summary>
        private readonly Func<String, Entity> _entitySpawner;


        /// <summary> Initialises the builder with the room-factory and entity-spawner delegates. </summary>
        /// <param name="roomFactory"> A delegate that builds a new <see cref="Room"/> from a room prefab name, returning <c>null</c> when the name is not registered. </param>
        /// <param name="entitySpawner"> A delegate that builds a new <see cref="Entity"/> from an entity prefab name. </param>
        public WorldBuilder(Func<String, Room?> roomFactory, Func<String, Entity> entitySpawner)
        {
            _roomFactory   = roomFactory;
            _entitySpawner = entitySpawner;
        }


        /// <summary> Executes the two-pass world-construction algorithm and returns the fully populated, connected rooms. </summary>
        /// <param name="definition"> The parsed world definition describing the rooms and connections to build. </param>
        /// <returns> An immutable collection of all constructed rooms, in the order they were declared in <paramref name="definition"/>. </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a <see cref="RoomInstanceSpec.PrefabName"/> is not registered, or when a <see cref="ConnectionSpec"/> references an instance id not found in the built room map.
        /// </exception>
        public IReadOnlyCollection<Room> Build(WorldDefinition definition)
        {
            Dictionary<String, Room> roomMap = BuildRoomMap(definition);
            WireConnections(definition, roomMap);

            return new List<Room>(roomMap.Values).AsReadOnly();
        }


        /// <summary> Pass 1: instantiates every room declared in <paramref name="definition"/> and populates each with its specified entities. </summary>
        /// <param name="definition"> The world definition whose room instance specs drive the pass. </param>
        /// <returns> A map from each instance id to its constructed <see cref="Room"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when a prefab name cannot be resolved; the message includes the offending instance id and prefab name. </exception>
        private Dictionary<String, Room> BuildRoomMap(WorldDefinition definition)
        {
            Dictionary<String, Room> roomMap = new Dictionary<String, Room>();

            foreach (RoomInstanceSpec spec in definition.Rooms)
            {
                Room room = InstantiateRoom(spec);
                PopulateRoom(room, spec);
                roomMap[spec.Id] = room;
            }

            return roomMap;
        }


        /// <summary> Creates a single room from the prefab named in <paramref name="spec"/>. </summary>
        /// <param name="spec"> The room instance spec whose <see cref="RoomInstanceSpec.PrefabName"/> is resolved via the room factory. </param>
        /// <returns> A newly constructed <see cref="Room"/> with all prefab features applied. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the room factory does not recognise <see cref="RoomInstanceSpec.PrefabName"/>; the message includes the instance id and prefab name. </exception>
        private Room InstantiateRoom(RoomInstanceSpec spec)
        {
            Room? room = _roomFactory(spec.PrefabName);

            if (room is null)
            {
                throw new InvalidOperationException(
                    $"Room instance '{spec.Id}': room prefab '{spec.PrefabName}' has not been loaded.");
            }

            return room;
        }


        /// <summary> Spawns each entity prefab named in <paramref name="spec"/> and adds the resulting entity to <paramref name="room"/>. </summary>
        /// <param name="room"> The room to receive the entities. </param>
        /// <param name="spec"> The room instance spec whose <see cref="RoomInstanceSpec.EntityPrefabNames"/> drive the spawning loop. </param>
        private void PopulateRoom(Room room, RoomInstanceSpec spec)
        {
            foreach (String entityPrefabName in spec.EntityPrefabNames)
            {
                Entity entity = _entitySpawner(entityPrefabName);
                room.AddEntity(entity);
            }
        }


        /// <summary> Pass 2: constructs a <see cref="Connection"/> for each <see cref="ConnectionSpec"/> and registers it on both room endpoints. </summary>
        /// <param name="definition"> The world definition whose connection specs drive the pass. </param>
        /// <param name="roomMap"> The map produced by Pass 1; both endpoint ids must be present. </param>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="ResolveEndpoint"/> when an endpoint id is absent from <paramref name="roomMap"/>. </exception>
        private static void WireConnections(WorldDefinition definition, Dictionary<String, Room> roomMap)
        {
            foreach (ConnectionSpec spec in definition.Connections)
            {
                Room roomFrom = ResolveEndpoint(roomMap, spec.FromId, spec);
                Room roomTo   = ResolveEndpoint(roomMap, spec.ToId,   spec);

                Connection connection = new Connection(roomFrom, roomTo);
                roomFrom.AddConnection(connection);
                roomTo.AddConnection(connection);
            }
        }


        /// <summary> Looks up a single connection endpoint by instance id in the room map, throwing with full context when the id is absent. </summary>
        /// <param name="roomMap"> The id-to-room map built during Pass 1. </param>
        /// <param name="instanceId"> The instance id to resolve. </param>
        /// <param name="spec"> The connection spec being processed; both endpoint ids are included in the error message. </param>
        /// <returns> The <see cref="Room"/> registered under <paramref name="instanceId"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="instanceId"/> is not present in <paramref name="roomMap"/>. </exception>
        private static Room ResolveEndpoint(Dictionary<String, Room> roomMap, String instanceId, ConnectionSpec spec)
        {
            Boolean found = roomMap.TryGetValue(instanceId, out Room? room);

            if (!found)
            {
                throw new InvalidOperationException(
                    $"Connection ('{spec.FromId}' -> '{spec.ToId}'): instance id '{instanceId}' was not found in the built room map.");
            }

            return room!;
        }
    }
}
