using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Rooms
{
    /// <summary> A connection that connects one room to another. </summary>
    public class Connection : IEquatable<Connection>
    {
        /// <summary> The two room connected by this connection. </summary>
        public readonly Room[] _rooms = new Room[2];


        /// <summary> Initialises a new instance of the <see cref="Connection"/> class. </summary>
        /// <param name="roomA"> The first of the two rooms connected by this connection. </param>
        /// <param name="roomB"> The second of the two rooms connected by this connection. </param>
        public Connection(Room roomA, Room roomB)
        {
            if (ReferenceEquals(roomA, roomB))
            {
                throw new ArgumentException($"Unable to create a connection between the same two rooms <{roomA.UId}, {roomB.UId}>.");
            }

            _rooms[0] = roomA;
            _rooms[1] = roomB;
        }


        /// <summary> Get an immutable array of the rooms connected by this connection. </summary>
        /// <returns> An immutable array of rooms connected by this connection. </returns>
        public IReadOnlyCollection<Room> GetRooms() => _rooms.ToArray();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => _rooms[0].GetHashCode() ^ _rooms[1].GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => Equals(obj as Connection);


        /// <inheritdoc/>
        public Boolean Equals(Connection? other) => other is not null
            && (ReferenceEquals(this, other)
                || _rooms[0].Equals(other._rooms[0]) && _rooms[1].Equals(other._rooms[1])
                || _rooms[0].Equals(other._rooms[1]) && _rooms[1].Equals(other._rooms[0]));
    }
}
