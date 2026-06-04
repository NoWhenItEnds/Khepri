using Khepri.Entities;
using Khepri.Entities.Definitions;
using Khepri.Rooms;
using Khepri.Rooms.Definitions;
using Khepri.Worlds.Definitions;
using System;
using System.Collections.Generic;

namespace Khepri.Worlds
{
    /// <summary> Constructs a live, connected world from one or more <see cref="WorldDefinition"/> resources in two deterministic passes: first instantiate and populate all rooms, then wire their connections. </summary>
    /// <remarks>
    /// All supplied definitions share a single room-instance id namespace, so a <see cref="RoomConnection"/> declared in one definition may reference a room declared in another. This mirrors how the entity and room prefab catalogues merge multiple source directories into one registry with global name uniqueness.
    /// <para>
    /// Pure two-pass orchestration — all instantiation and registration is delegated to the manager layer via three injected seams:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>roomFactory</c> — builds and registers a <see cref="Room"/> (provided by <c>RoomManager.CreateRoom</c>).</description></item>
    ///   <item><description><c>entitySpawner</c> — builds and registers an <see cref="Entity"/> (provided by <c>EntityManager.CreateEntity</c>).</description></item>
    ///   <item><description><c>connectionLinker</c> — constructs and wires a position-aware, distance-weighted connection between two rooms (provided by <c>RoomManager.AddConnection</c>); receives roomA, positionA, roomB, positionB, distance.</description></item>
    /// </list>
    /// Pass 1 — for each <see cref="RoomInstance"/> across every definition, validate the id, delegate room creation to <c>roomFactory</c>, then spawn each <see cref="EntityPlacement"/> via <c>entitySpawner</c>.
    /// Pass 2 — for each <see cref="RoomConnection"/> across every definition, look up both endpoints by id and delegate connection wiring to <c>connectionLinker</c>.
    /// </remarks>
    public sealed class WorldBuilder
    {
        /// <summary> Builds and registers a <see cref="Room"/> from the given prefab; provided by <c>RoomManager.CreateRoom</c>. </summary>
        private readonly Func<RoomPrefab, Room> _roomFactory;

        /// <summary> Builds and registers a freshly spawned <see cref="Entity"/> from the given prefab; provided by <c>EntityManager.CreateEntity</c>. </summary>
        private readonly Func<EntityPrefab, Entity> _entitySpawner;

        /// <summary> Constructs a position-aware, distance-weighted bidirectional connection between two already-registered rooms and wires it on both endpoints; provided by <c>RoomManager.AddConnection</c>. Parameters are: roomA, positionA, roomB, positionB, distance. </summary>
        private readonly Action<Room, RoomPosition, Room, RoomPosition, Single> _connectionLinker;


        /// <summary> Initialises the builder with the three manager-layer delegates that handle all instantiation and registration. </summary>
        /// <param name="roomFactory"> Creates and registers a <see cref="Room"/> from the given <see cref="RoomPrefab"/>. </param>
        /// <param name="entitySpawner"> Creates and registers an <see cref="Entity"/> from the given <see cref="EntityPrefab"/>. </param>
        /// <param name="connectionLinker"> Constructs and registers a position-aware, distance-weighted bidirectional connection; receives roomA, positionA, roomB, positionB, distance. </param>
        public WorldBuilder(
            Func<RoomPrefab, Room>                                  roomFactory,
            Func<EntityPrefab, Entity>                              entitySpawner,
            Action<Room, RoomPosition, Room, RoomPosition, Single>  connectionLinker)
        {
            _roomFactory      = roomFactory;
            _entitySpawner    = entitySpawner;
            _connectionLinker = connectionLinker;
        }


        /// <summary> Executes the two-pass world-construction algorithm over every supplied definition and returns the fully populated, connected rooms. </summary>
        /// <param name="definitions"> The world definition resources describing the rooms and connections to build; all share a single room-instance id namespace. </param>
        /// <returns> An immutable collection of all constructed rooms, in declaration order. </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a <see cref="RoomInstance.Id"/> is blank or duplicated across the supplied definitions, when <see cref="RoomInstance.Prefab"/> or an <see cref="EntityPlacement.Prefab"/> is null, or when a <see cref="RoomConnection"/> references an instance id absent from the built room map.
        /// </exception>
        public IReadOnlyCollection<Room> Build(IEnumerable<WorldDefinition> definitions)
        {
            Dictionary<String, Room> roomMap = BuildRoomMap(definitions);
            WireConnections(definitions, roomMap);

            return new List<Room>(roomMap.Values).AsReadOnly();
        }


        /// <summary> Pass 1: validates ids, delegates room creation to <see cref="_roomFactory"/>, and populates each room with its entity placements, merging every definition into one id namespace. </summary>
        /// <param name="definitions"> The world definitions whose room instances drive the pass. </param>
        /// <returns> A map from each instance id to its constructed <see cref="Room"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when any room id is blank, any id is duplicated across the definitions, or any prefab reference is null. </exception>
        private Dictionary<String, Room> BuildRoomMap(IEnumerable<WorldDefinition> definitions)
        {
            Dictionary<String, Room> roomMap = new Dictionary<String, Room>();

            foreach (WorldDefinition definition in definitions)
            {
                foreach (RoomInstance spec in definition.Rooms)
                {
                    Boolean idBlank = String.IsNullOrWhiteSpace(spec.Id);

                    if (idBlank)
                    {
                        throw new InvalidOperationException(
                            "A RoomInstance has a blank Id. Every room instance must have a unique, non-blank identifier across all world definitions.");
                    }

                    Boolean duplicate = roomMap.ContainsKey(spec.Id);

                    if (duplicate)
                    {
                        throw new InvalidOperationException(
                            $"Duplicate room instance id '{spec.Id}'. Each room instance must have a unique id across all world definitions.");
                    }

                    Room room = InstantiateRoom(spec);
                    PopulateRoom(room, spec);
                    roomMap[spec.Id] = room;
                }
            }

            return roomMap;
        }


        /// <summary> Delegates creation of a single room to <see cref="_roomFactory"/> after validating the prefab reference. </summary>
        /// <param name="spec"> The room instance whose <see cref="RoomInstance.Prefab"/> is passed to the factory. </param>
        /// <returns> A newly constructed, manager-registered <see cref="Room"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="RoomInstance.Prefab"/> is null; the message names the instance id. </exception>
        private Room InstantiateRoom(RoomInstance spec)
        {
            Boolean prefabMissing = spec.Prefab is null;

            if (prefabMissing)
            {
                throw new InvalidOperationException(
                    $"Room instance '{spec.Id}' has no Prefab assigned. Set the Prefab field in the WorldDefinition resource.");
            }

            return _roomFactory(spec.Prefab!);
        }


        /// <summary> Spawns each <see cref="EntityPlacement"/> declared in <paramref name="spec"/> and adds the resulting entity to <paramref name="room"/> at the placement's declared position. </summary>
        /// <param name="room"> The room to receive the entities. </param>
        /// <param name="spec"> The room instance whose <see cref="RoomInstance.Entities"/> drive the spawning loop. </param>
        /// <exception cref="InvalidOperationException"> Thrown when any <see cref="EntityPlacement.Prefab"/> is null; the message names the owning room instance id. </exception>
        private void PopulateRoom(Room room, RoomInstance spec)
        {
            foreach (EntityPlacement placement in spec.Entities)
            {
                Boolean prefabMissing = placement.Prefab is null;

                if (prefabMissing)
                {
                    throw new InvalidOperationException(
                        $"Room instance '{spec.Id}' contains an EntityPlacement with no Prefab assigned. Set the Prefab field on every EntityPlacement in the WorldDefinition resource.");
                }

                Entity entity = _entitySpawner(placement.Prefab!);
                room.AddEntity(entity, placement.Position);
            }
        }


        /// <summary> Pass 2: delegates connection wiring to <see cref="_connectionLinker"/> for each <see cref="RoomConnection"/> across every definition. </summary>
        /// <param name="definitions"> The world definitions whose connection entries drive the pass. </param>
        /// <param name="roomMap"> The map produced by Pass 1; both endpoint ids must be present. </param>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="ResolveEndpoint"/> when an endpoint id is absent from <paramref name="roomMap"/>. </exception>
        private void WireConnections(IEnumerable<WorldDefinition> definitions, Dictionary<String, Room> roomMap)
        {
            foreach (WorldDefinition definition in definitions)
            {
                foreach (RoomConnection spec in definition.Connections)
                {
                    Room roomFrom = ResolveEndpoint(roomMap, spec.FromId, spec);
                    Room roomTo   = ResolveEndpoint(roomMap, spec.ToId,   spec);

                    _connectionLinker(roomFrom, spec.FromPosition, roomTo, spec.ToPosition, spec.Distance);
                }
            }
        }


        /// <summary> Looks up a single connection endpoint by instance id in the room map, throwing with full context when the id is absent. </summary>
        /// <param name="roomMap"> The id-to-room map built during Pass 1. </param>
        /// <param name="instanceId"> The instance id to resolve. </param>
        /// <param name="spec"> The connection spec being processed; both endpoint ids are included in the error message. </param>
        /// <returns> The <see cref="Room"/> registered under <paramref name="instanceId"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="instanceId"/> is not present in <paramref name="roomMap"/>. </exception>
        private static Room ResolveEndpoint(Dictionary<String, Room> roomMap, String instanceId, RoomConnection spec)
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
