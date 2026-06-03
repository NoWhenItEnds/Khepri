using System;
using Khepri.Prefabs;

namespace Khepri.Rooms.Features
{
    /// <summary> A single aspect or characteristic of a room. A room's properties are defined by the features attached to it. </summary>
    /// <remarks> Equality is type-based — a room can hold at most one feature of each concrete type. </remarks>
    public abstract class Feature : IEquatable<Feature>, IPart<Room>
    {
        /// <summary> The room that this feature is attached to. </summary>
        public readonly Room ParentRoom;


        /// <inheritdoc/>
        Room IPart<Room>.Owner => ParentRoom;


        /// <summary> Initialises a new instance attached to the given room. </summary>
        /// <param name="parentRoom"> The room that this feature belongs to. </param>
        public Feature(Room parentRoom)
        {
            ParentRoom = parentRoom;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => GetType().GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Feature other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Feature? other) => other is not null && GetType().Equals(other.GetType());
    }
}
