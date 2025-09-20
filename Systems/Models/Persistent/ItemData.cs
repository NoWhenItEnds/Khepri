using System;
using System.Text.Json.Serialization;
using System.Threading;
using Godot;

namespace Khepri.Models.Persistent
{
    /// <summary> The persistent stats of an item. These are saved and loaded to keep track of the world. </summary>
    public record ItemData : IPersistent
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
        public Int32 CompareTo(IPersistent other) => UId.CompareTo(other.UId);
    }
}
