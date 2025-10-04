using Godot;
using System;

namespace Khepri.Entities
{
    /// <summary> A structural prop that acts as a wall, either man-made or natural. </summary>
    public partial class Wall : StaticBody3D, ITileable
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();  // TODO - Pull from generator.

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Wall? other = obj as Wall;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
