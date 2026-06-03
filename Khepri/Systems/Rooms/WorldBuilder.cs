using Khepri.Entities;
using Khepri.Prefabs;
using Khepri.Rooms.Features;
using Khepri.Rooms.Prefabs;
using System;
using System.Collections.Generic;

namespace Khepri.Rooms
{
    /// <summary> Constructs a live, connected world from a parsed <see cref="WorldDefinition"/> in two deterministic passes: first instantiate and populate all rooms, then wire their connections. </summary>
    /// <remarks>
    /// Godot-free and decoupled from <c>EntityManager</c>. The entity-spawning responsibility is delegated to the <c>entitySpawner</c> delegate injected at construction time, which is the sole DIP seam between this POCO layer and the Godot layer above it.
    /// Pass 1 — for each <see cref="RoomInstanceSpec"/>, resolve its room prefab via the catalogue, build the room via <see cref="RoomFactory.CreateFrom"/>, record the instance id-to-<see cref="Room"/> mapping, then invoke the spawner delegate for each entity prefab name and add the resulting <see cref="Entity"/> to the room.
    /// Pass 2 — for each <see cref="ConnectionSpec"/>, look up both rooms by instance id, construct a <see cref="Connection"/>, and register it on both room endpoints via <see cref="Room.AddConnection"/>.
    /// </remarks>
    public sealed class WorldBuilder
    {
        /// <summary> The catalogue consulted when resolving a room prefab name to a <see cref="Prefab{TOwner,TPart}"/>. </summary>
        private readonly RoomCatalogue _catalogue;

        /// <summary> The delegate that maps an entity prefab name to a freshly spawned <see cref="Entity"/>; provided by the Godot layer so this class stays Godot-free. </summary>
        private readonly Func<String, Entity> _entitySpawner;


        /// <summary> Initialises the builder with the room catalogue and the entity-spawner delegate. </summary>
        /// <param name="catalogue"> The catalogue used to resolve room prefab names during Pass 1. </param>
        /// <param name="entitySpawner"> A delegate that accepts an entity prefab name and returns a fully constructed <see cref="Entity"/>; the Godot layer passes <c>EntityManager.Instance.CreateEntityFromPrefab</c> here. </param>
        public WorldBuilder(RoomCatalogue catalogue, Func<String, Entity> entitySpawner)
        {
            _catalogue     = catalogue;
            _entitySpawner = entitySpawner;
        }


        /// <summary> Executes the two-pass world-construction algorithm and returns the fully populated, connected rooms. </summary>
        /// <param name="definition"> The parsed world definition describing the rooms and connections to build. </param>
        /// <returns> An immutable collection of all constructed rooms, in the order they were declared in <paramref name="definition"/>. </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when:
        /// <list type="bullet">
        ///   <item><description>a <see cref="RoomInstanceSpec.PrefabName"/> is not present in the catalogue; the message includes the offending instance id and prefab name;</description></item>
        ///   <item><description>a <see cref="ConnectionSpec"/> references an instance id not found in the built room map; the message names both the offending id and the full connection.</description></item>
        /// </list>
        /// </exception>
        public IReadOnlyCollection<Room> Build(WorldDefinition definition)
        {
            Dictionary<String, Room> roomMap = BuildRoomMap(definition);
            WireConnections(definition, roomMap);

            return new List<Room>(roomMap.Values).AsReadOnly();
        }


        /// <summary> Pass 1: instantiates every room declared in <paramref name="definition"/> and populates each with its specified entities, recording the instance-id to <see cref="Room"/> mapping for use in Pass 2. </summary>
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
        /// <param name="spec"> The room instance spec whose <see cref="RoomInstanceSpec.PrefabName"/> is resolved from the catalogue. </param>
        /// <returns> A newly constructed <see cref="Room"/> with all prefab features applied. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="RoomInstanceSpec.PrefabName"/> is not present in the catalogue; the message includes the instance id and prefab name for actionable diagnosis. </exception>
        private Room InstantiateRoom(RoomInstanceSpec spec)
        {
            Boolean found = _catalogue.TryGet(spec.PrefabName, out Prefab<Room, Feature>? prefab);

            if (!found)
            {
                throw new InvalidOperationException(
                    $"Room instance '{spec.Id}': room prefab '{spec.PrefabName}' has not been loaded into the catalogue.");
            }

            return RoomFactory.CreateFrom(prefab!);
        }


        /// <summary> Invokes the entity spawner for each entity prefab name in <paramref name="spec"/> and adds the resulting entity to <paramref name="room"/>. </summary>
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
        /// <param name="spec"> The connection spec being processed; both endpoint ids are included in the error message so the caller can locate the offending connection in the definition. </param>
        /// <returns> The <see cref="Room"/> registered under <paramref name="instanceId"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="instanceId"/> is not present in <paramref name="roomMap"/>; the message names the offending id and both endpoints of the connection. </exception>
        private static Room ResolveEndpoint(Dictionary<String, Room> roomMap, String instanceId, ConnectionSpec spec)
        {
            Boolean found = roomMap.TryGetValue(instanceId, out Room? room);

            if (!found)
            {
                throw new InvalidOperationException(
                    $"Connection ('{spec.FromId}' -> '{spec.ToId}'): instance id '{instanceId}' was not found in the built room map. Ensure the world definition loader has validated all connection endpoints before calling Build.");
            }

            return room!;
        }
    }
}
