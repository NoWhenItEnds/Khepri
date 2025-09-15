using Godot;
using System;

namespace Khepri.Entities
{
    /// <summary> A static piece of the terrain that can be walked on such as earth or flooring. </summary>
    public partial class Terrain : StaticBody3D, IEntity, ITileable
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
