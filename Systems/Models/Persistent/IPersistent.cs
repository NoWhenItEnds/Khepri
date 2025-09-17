using Godot;
using System;
using System.Threading;

namespace Khepri.Models.Persistent
{
    /// <summary> A piece of data that represents a set of persistent data that can be saved and loaded. </summary>
    public interface IPersistent : IComparable<IPersistent>
    {
        /// <summary> The object's unique identifier. Acts as it's key value when mapping data to it. </summary>
        public Guid UId { get; }

        /// <summary> The location of the entity in world space. </summary>
        public Vector3 WorldPosition { get; }

        /// <summary> Save the persistent object to the datastore. </summary>
        /// <param name="cancellationToken"> Gracefully stop the operation. </param>
        public void SaveAsync(CancellationToken cancellationToken);
    }
}
