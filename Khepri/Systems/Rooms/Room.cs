using Khepri.Entities;
using System;
using System.Collections.Generic;

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


        /// <inheritdoc/>
        public override Int32 GetHashCode() => UId.GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Entity other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Room? other) => other is not null && UId.Equals(other.UId);
    }
}
