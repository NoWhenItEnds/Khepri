using Khepri.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Rooms
{
    /// <summary> A single self-contained playable space within the game world. It has entities within it, and connects to other rooms. </summary>
    public class Room : IEquatable<Room>
    {
        /// <summary> The room's unique identifier. Should be unique across all rooms. </summary>
        public readonly Guid UId;


        /// <summary> The connections branching off from this current room. </summary>
        private readonly HashSet<Connection> _connections = new HashSet<Connection>();

        /// <summary> The entities directly in the current room. </summary>
        /// <remarks> These are the entities that are directly present, not also those entities within their parent entities. </remarks>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();


        /// <summary> Initialises a new instance of the <see cref="Room"/> class. </summary>
        /// <param name="uid"> The unique identifier for the room. </param>
        public Room(Guid uid)
        {
            UId = uid;
        }


        /// <summary> Build a dynamic description of the room's current state to display to the player. </summary>
        /// <returns> A richly formatted string to display to the player representing the room's current state / situation. </returns>
        public String BuildDescription()
        {
            return "This is a room.";   // TODO - Figure out how to do this!
        }


        /// <summary> Attempt to add an entity to the entities currently within the room. </summary>
        /// <param name="entity"> The entity to move into the room. </param>
        /// <returns> Whether the entity was successfully added to the room. </returns>
        public Boolean AddEntity(Entity entity) => _entities.Add(entity);


        /// <summary> Attempt to remove an entity from the entities currently within the room. </summary>
        /// <param name="entity"> The entity to remove from the room. </param>
        /// <returns> Whether the entity was successfully removed to the room. </returns>
        public Boolean RemoveEntity(Entity entity) => _entities.Remove(entity);


        /// <summary> Get an immutable array of the entities within this room. </summary>
        /// <returns> The entities currently in this room. </returns>
        public Entity[] GetEntities() => _entities.ToArray();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => UId.GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Entity other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Room? other) => other is not null && UId.Equals(other.UId);
    }
}
