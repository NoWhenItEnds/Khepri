using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Interfaces;
using Khepri.Models.Persistent;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A node representing an item in the game world. </summary>
    public partial class Item : Node3D, IEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private AnimatedSprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <inheritdoc/>
        public Guid UId => Data.UId;

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <inheritdoc/>
        public ItemData Data { get; private set; } = new ItemData();


        /// <inheritdoc/>
        public override void _Ready()
        {
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;
        }


        private void OnBodyEntered(Node3D body)
        {
            if (body is Unit unit)
            {
                unit.AddUsableEntity(this);
            }
        }


        private void OnBodyExited(Node3D body)
        {
            if (body is Unit unit)
            {
                unit.RemoveUsableEntity(this);
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            Double bobOffset = Math.Sin(Engine.GetFramesDrawn() * 0.01f) * 2f;
            _sprite.Offset = new Vector2(0f, (Single)bobOffset);
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Data.UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Item? other = obj as Item;
            return other != null ? Data.UId.Equals(other.Data.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
