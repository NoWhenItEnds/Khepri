using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Extensions.Logging;
using Jaypen.Utilities.Extensions;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Rooms;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing rooms, their prefab catalogue, and the connections between them. </summary>
    public partial class RoomManager : SingletonNode<RoomManager>
    {
        /// <summary> The Godot resource directories to scan for room prefab (<c>*.tres</c>) definitions. </summary>
        /// <remarks> Paths are Godot resource paths (e.g. <c>res://Khepri/Data/Prefabs/Rooms</c>). Every <c>.tres</c> in each directory is loaded as a <see cref="RoomPrefab"/>; prefab names must be unique across all directories. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>
        {
            "res://Khepri/Data/Prefabs/Rooms"
        };


        /// <summary> All loaded room prefabs, keyed by their <see cref="RoomPrefab.Name"/>. </summary>
        private readonly Dictionary<String, RoomPrefab> _prefabsByName = new Dictionary<String, RoomPrefab>();

        /// <summary> All rooms that currently exist within the game world. </summary>
        private readonly HashSet<Room> _rooms = new HashSet<Room>();

        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<RoomManager>();


        /// <summary> Loads every room prefab resource from the configured directories and registers each by name. </summary>
        public override void _Ready()
        {
            foreach (String path in _prefabPaths)
            {
                foreach (RoomPrefab prefab in ResourceExtensions.GetResources<RoomPrefab>(path))
                {
                    Register(prefab, prefab.ResourcePath);
                }
            }
        }


        /// <summary> Instantiates the given prefab, registers the resulting room in the live room set, and returns it. </summary>
        /// <remarks> This is the seam passed to <see cref="Rooms.WorldBuilder"/> as its <c>roomFactory</c> delegate, so rooms are registered as they are built during world construction. </remarks>
        /// <param name="prefab"> The room prefab to instantiate; must not be null. </param>
        /// <returns> The newly constructed, registered room. </returns>
        public Room CreateRoom(RoomPrefab prefab)
        {
            Room room = prefab.Instantiate();
            _rooms.Add(room);

            return room;
        }


        /// <summary> Resolves a prefab by name and delegates to <see cref="CreateRoom(RoomPrefab)"/> to build and register the room. </summary>
        /// <remarks> The world-construction build path uses direct prefab references via <see cref="CreateRoom(RoomPrefab)"/>, so this by-name entry point is currently unused by <see cref="Rooms.WorldBuilder"/>. It exists for symmetry with <see cref="EntityManager.CreateEntity(String)"/> and for future by-name creation scenarios. </remarks>
        /// <param name="prefabName"> The name of the prefab to instantiate; must exist in a loaded directory. </param>
        /// <returns> The newly constructed, registered room. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when no prefab with <paramref name="prefabName"/> has been loaded. </exception>
        public Room CreateRoom(String prefabName)
        {
            Boolean found = _prefabsByName.TryGetValue(prefabName, out RoomPrefab? prefab);

            if (!found)
            {
                throw new KeyNotFoundException($"No room prefab named '{prefabName}' has been loaded.");
            }

            return CreateRoom(prefab!);
        }


        /// <summary> Creates a position-aware, distance-weighted bidirectional <see cref="Connection"/> between two room endpoints and registers it on both. </summary>
        /// <remarks> Also serves as the <c>connectionLinker</c> seam passed to <see cref="Rooms.WorldBuilder"/>. Both rooms must already be registered via <see cref="CreateRoom(Definitions.RoomPrefab)"/>. </remarks>
        /// <param name="roomA"> The first room endpoint; must already be tracked by this manager. </param>
        /// <param name="positionA"> The position within <paramref name="roomA"/> at which this connection attaches. </param>
        /// <param name="roomB"> The second room endpoint; must already be tracked by this manager. </param>
        /// <param name="positionB"> The position within <paramref name="roomB"/> at which this connection attaches. </param>
        /// <param name="distance"> The travel cost through the connection in metres; must be zero or positive. Defaults to 0. </param>
        /// <exception cref="ArgumentException"> Thrown when either room is not tracked by this manager. Propagated from <see cref="Connection"/> when both endpoints are the same room at the same position, or when <paramref name="distance"/> is negative. </exception>
        public void AddConnection(Room roomA, RoomPosition positionA, Room roomB, RoomPosition positionB, Single distance = 0f)
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

            Connection connection = new Connection(roomA, positionA, roomB, positionB, distance);
            roomA.AddConnection(connection);
            roomB.AddConnection(connection);
        }


        /// <summary> Returns all rooms currently in the game world. </summary>
        /// <returns> An immutable snapshot of all registered rooms; never null, but may be empty before world construction has run. </returns>
        public IReadOnlyCollection<Room> GetRooms() => _rooms.ToArray();


        /// <summary> Returns the room that contains the given entity, searching the full <see cref="IEntityContainer"/> hierarchy. </summary>
        /// <param name="entity"> The entity to locate. </param>
        /// <returns> The room whose containment tree includes <paramref name="entity"/> at any depth, or <c>null</c> if the entity occupies no tracked room. </returns>
        /// <remarks> An entity belonging to no room is abnormal, so this logs a warning and returns <c>null</c>, letting callers degrade gracefully rather than crashing the game. </remarks>
        public Room? GetCurrentRoom(Entity entity)
        {
            Room? result = _rooms.FirstOrDefault(room => ((IEntityContainer)room).Contains(entity));

            if (result is null)
            {
                Logger.LogWarning("Entity ({Uid}) could not be located in any room.", entity.UId);
            }

            return result;
        }


        /// <summary> Moves <paramref name="entity"/> from its current room into <paramref name="destination"/>, provided the two rooms are directly connected. </summary>
        /// <remarks>
        /// This is the single shared move primitive: the player's UI and AI controllers both funnel through it so
        /// movement rules live in one place. The entity arrives at the <see cref="RoomPosition"/> where the connecting
        /// link attaches within <paramref name="destination"/> — i.e. at the doorway it came through. Only
        /// directly-connected rooms are reachable in one call; multi-room travel is a higher-level concern (pathfinding)
        /// that would call this once per step.
        /// </remarks>
        /// <param name="entity"> The entity to move; must currently occupy a tracked room. </param>
        /// <param name="destination"> The room to move the entity into. </param>
        /// <returns> <c>true</c> if the move happened; <c>false</c> if the entity occupies no room, is already there, or no connection links the two rooms. </returns>
        public Boolean MoveEntity(Entity entity, Room destination)
        {
            Boolean moved   = false;
            Room?   current = GetCurrentRoom(entity);

            if (current is not null && !current.Equals(destination))
            {
                Connection? link = current.GetConnections()
                    .FirstOrDefault(connection => connection.GetRooms().Any(room => room.Equals(destination)));

                if (link is not null)
                {
                    RoomPosition arrival = link.GetPositions(destination).FirstOrDefault();

                    current.RemoveEntity(entity);
                    destination.AddEntity(entity, arrival);
                    moved = true;
                }
            }

            return moved;
        }


        /// <summary> Registers a single room prefab by its name. </summary>
        /// <remarks> A blank or duplicate name is a single-file authoring slip, so it is logged and skipped rather than crashing the boot; the first prefab registered under a name wins. </remarks>
        /// <param name="prefab"> The loaded prefab to register. </param>
        /// <param name="resourcePath"> The resource path, used to enrich the skip log. </param>
        private void Register(RoomPrefab prefab, String resourcePath)
        {
            if (String.IsNullOrWhiteSpace(prefab.Name))
            {
                Logger.LogError("Skipping room prefab '{Path}': it has a blank Name.", resourcePath);
            }
            else if (_prefabsByName.ContainsKey(prefab.Name))
            {
                Logger.LogError("Skipping duplicate room prefab name '{Name}' (from '{Path}'); keeping the one already registered.", prefab.Name, resourcePath);
            }
            else
            {
                _prefabsByName[prefab.Name] = prefab;
            }
        }
    }
}
