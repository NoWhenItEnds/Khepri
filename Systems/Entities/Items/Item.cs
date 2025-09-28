using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Interfaces;
using Khepri.Entities.Items.Components;
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


        /// <summary> The data component representing the item's data. </summary>
        public ItemDataComponent Data { get; private set; }


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <summary> A reference to the item factory. </summary>
        private ItemFactory _itemFactory;


        /// <summary> Initialise the item by giving data. </summary>
        /// <param name="data"> The raw data to build the item. </param>
        public void Initialise(ItemDataComponent data)
        {
            Data = data;
        }


        /// <summary> Using an item tries to pick it up. </summary>
        public void Use(IEntity activatingEntity)
        {
            if (activatingEntity is Unit unit)
            {
                Boolean isSuccessful = unit.Inventory.AddItem(Data);
                if (isSuccessful)   // If the item was added, free it back to the pool.
                {
                    _itemFactory.FreeItem(this);
                }
            }
        }


        /// <inheritdoc/>
        public override void _Ready()
        {
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;

            _itemFactory = ItemFactory.Instance;
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
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Item? other = obj as Item;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
