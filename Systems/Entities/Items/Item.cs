using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.Entities.Interfaces;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A node representing an item in the game world. </summary>
    public partial class Item : StaticBody3D, IEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private AnimatedSprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <summary> The data component representing the item's data. </summary>
        public ItemData Data { get; private set; }


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <summary> A reference to the item controller. </summary>
        private ItemController _itemController;


        /// <summary> Initialise the item by giving data. </summary>
        /// <param name="data"> The raw data to build the item. </param>
        public void Initialise(ItemData data)
        {
            Data = data;
        }


        /// <summary> Using an item tries to pick it up. </summary>
        public void Use(IEntity activatingEntity)
        {
            if (activatingEntity is Unit unit)
            {
                Boolean isSuccessful = unit.Inventory.TryAddItem(Data);
                if (isSuccessful)   // If the item was added, free it back to the pool.
                {
                    _itemController.FreeItem(this);
                }
            }
        }


        /// <inheritdoc/>
        public override void _Ready()
        {
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;

            _itemController = ItemController.Instance;
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

            _sprite.SortingOffset = (Int32)GlobalPosition.Y * 10000 + GlobalPosition.Z;
        }
    }
}
