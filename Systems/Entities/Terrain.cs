using Godot;
using Khepri.Entities.Interfaces;
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
        public Guid UId { get; } = Guid.NewGuid();  // TODO - Pull from generator.

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Terrain? other = obj as Terrain;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
