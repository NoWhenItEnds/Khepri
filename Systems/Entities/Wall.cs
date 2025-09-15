using Godot;
using System;

namespace Khepri.Entities
{
    /// <summary> A structural prop that acts as a wall, either man-made or natural. </summary>
    public partial class Wall : RigidBody3D, IEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }


        /// <inheritdoc/>
        public Guid UId { get; private set; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <inheritdoc/>
        public int CompareTo(IEntity other)
        {
            return UId.CompareTo(other.UId);
        }
    }
}
