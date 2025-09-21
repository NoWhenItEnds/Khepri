using System;
using System.Text.Json.Serialization;
using System.Threading;
using Godot;

namespace Khepri.Models.Persistent
{
    /// <summary> The persistent stats of an item. These are saved and loaded to keep track of the world. </summary>
    public class ItemData : IPersistent
    {
        /// <inheritdoc/>
        [JsonPropertyName("uid")]
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        [JsonPropertyName("position")]
        public Vector3 WorldPosition { get; private set; } = Vector3.Zero;   // TODO - Figure out what to do here.


        /// <inheritdoc/>
        public void SaveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            ItemData? other = obj as ItemData;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IPersistent other) => UId.Equals(other.UId);
    }
}
