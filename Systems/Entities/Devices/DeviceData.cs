using Godot;
using System;

namespace Khepri.Entities.Devices
{
    public abstract class DeviceData : IEquatable<DeviceData>
    {
        /// <summary> The unique identifier of this item instance. </summary>
        public Guid UId { get; init; }

        /// <summary> The unique identifying name or key of the item. </summary>
        public String Name { get; init; }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            DeviceData? other = obj as DeviceData;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(DeviceData other) => UId.Equals(other.UId);
    }
}
