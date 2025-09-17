using System;
using System.Threading;
using Godot;

namespace Khepri.Models.Persistent
{
    /// <summary> The persistent stats of a unit. These are saved and loaded to keep track of the world. </summary>
    public class UnitStats : IPersistent
    {
        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition { get; }   // TODO - Figure out what to do here.

        /// <summary> The unit's base movement speed. </summary>
        public Single BaseSpeed { get; private set; } = 3f;

        /// <summary> The amount to modify the base speed amount for sprinting. </summary>
        public Single SprintModifier { get; private set; } = 2f;


        /// <inheritdoc/>
        public void SaveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public Int32 CompareTo(IPersistent other) => UId.CompareTo(other.UId);
    }
}
