using Godot;
using Khepri.Entities.Interfaces;
using Khepri.Models.Persistent;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A food item that can be used to reduce hunger. </summary>
    public partial class Food : RigidBody3D, IEntity, IItem
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <inheritdoc/>
        public Guid UId => Data.UId;

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <inheritdoc/>
        public ItemData Data { get; private set; } = new ItemData();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Data.UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Food? other = obj as Food;
            return other != null ? Data.UId.Equals(other.Data.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
