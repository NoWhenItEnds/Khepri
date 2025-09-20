using Godot;
using Khepri.Entities.Interfaces;
using Khepri.Models.Persistent;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A food item that can be used to reduce hunger. </summary>
    public partial class Food : RigidBody3D, ISmartEntity, IItem
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }


        /// <inheritdoc/>
        public Guid UId { get; private set; } = Guid.NewGuid(); // TODO - This should be a part of persistent stats.

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;

        /// <inheritdoc/>
        public ItemData Data { get; private set; } = new ItemData();


        /// <inheritdoc/>
        public int CompareTo(IEntity other)
        {
            return UId.CompareTo(other.UId);
        }
    }
}
