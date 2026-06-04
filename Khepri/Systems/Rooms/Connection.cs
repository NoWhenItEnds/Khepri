using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Rooms
{
    /// <summary> A bidirectional connection between two room endpoints, each identified by a <see cref="Room"/> and a <see cref="RoomPosition"/> within that room. </summary>
    /// <remarks> Both endpoints are stored at index 0 and 1 in parallel arrays: <c>_rooms[i]</c> and <c>_positions[i]</c> always describe the same endpoint. </remarks>
    public class Connection : IEquatable<Connection>
    {
        /// <summary> The two rooms joined by this connection, indexed in parallel with <see cref="_positions"/>. </summary>
        private readonly Room[] _rooms = new Room[2];

        /// <summary> The position within each room at which this connection attaches, indexed in parallel with <see cref="_rooms"/>. </summary>
        private readonly RoomPosition[] _positions = new RoomPosition[2];

        /// <summary> The travel cost through this connection in metres; used by pathfinding. Zero by default. Not part of identity — two connections with the same endpoints but different distances are considered equal. </summary>
        public readonly Single Distance;


        /// <summary> Initialises a new connection between the given (room, position) endpoints with an optional travel distance. </summary>
        /// <param name="roomA"> The first room endpoint. </param>
        /// <param name="positionA"> The position within <paramref name="roomA"/> at which this connection attaches. </param>
        /// <param name="roomB"> The second room endpoint. </param>
        /// <param name="positionB"> The position within <paramref name="roomB"/> at which this connection attaches. </param>
        /// <param name="distance"> The travel cost in metres; must be zero or positive. Defaults to 0. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when both endpoints are the same room instance AND the same position (the degenerate self-at-same-slot case), or when <paramref name="distance"/> is negative.
        /// </exception>
        public Connection(Room roomA, RoomPosition positionA, Room roomB, RoomPosition positionB, Single distance = 0f)
        {
            if (ReferenceEquals(roomA, roomB) && positionA == positionB)
            {
                throw new ArgumentException(
                    $"Cannot create a connection from a room slot to itself: room '{roomA.UId}', position '{positionA}'.");
            }

            if (distance < 0f)
            {
                throw new ArgumentException(
                    $"Connection distance cannot be negative (got {distance}m).", nameof(distance));
            }

            _rooms[0]     = roomA;
            _positions[0] = positionA;
            _rooms[1]     = roomB;
            _positions[1] = positionB;
            Distance      = distance;
        }


        /// <summary> Returns the two rooms joined by this connection; both entries are the same room for a self-connection. </summary>
        /// <returns> An immutable collection of the two room endpoints. </returns>
        public IReadOnlyCollection<Room> GetRooms() => _rooms.ToArray();


        /// <summary> Returns the position(s) at which this connection attaches within <paramref name="room"/>. </summary>
        /// <remarks> Returns one position for a normal connection; two positions when <paramref name="room"/> is both endpoints (a self-connection). Returns an empty collection when <paramref name="room"/> is not an endpoint of this connection. </remarks>
        /// <param name="room"> The room whose attachment positions are requested. </param>
        /// <returns> An immutable collection of positions; contains one entry normally, two for a self-connection, and zero if the room is unrelated. </returns>
        public IReadOnlyCollection<RoomPosition> GetPositions(Room room)
        {
            List<RoomPosition> result = new List<RoomPosition>();

            for (Int32 i = 0; i < 2; i++)
            {
                if (_rooms[i].Equals(room))
                {
                    result.Add(_positions[i]);
                }
            }

            return result.AsReadOnly();
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() =>
            (_rooms[0].GetHashCode() ^ (Int32)_positions[0]) ^
            (_rooms[1].GetHashCode() ^ (Int32)_positions[1]);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => Equals(obj as Connection);


        /// <inheritdoc/>
        public Boolean Equals(Connection? other)
        {
            Boolean sameOrder = other is not null
                && _rooms[0].Equals(other._rooms[0]) && _positions[0] == other._positions[0]
                && _rooms[1].Equals(other._rooms[1]) && _positions[1] == other._positions[1];

            Boolean swappedOrder = other is not null
                && _rooms[0].Equals(other._rooms[1]) && _positions[0] == other._positions[1]
                && _rooms[1].Equals(other._rooms[0]) && _positions[1] == other._positions[0];

            return other is not null && (ReferenceEquals(this, other) || sameOrder || swappedOrder);
        }
    }
}
