using Godot;
using Khepri.Data;
using Khepri.Data.Actors;
using Khepri.Data.Items;
using Khepri.Entities.Actors;
using Khepri.Types;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A node representing an item in the game world. </summary>
    public partial class ItemNode : StaticBody3D, IEntity, IPoolable
    {
        /// <inheritdoc/>
        public Guid DataUId => _data.UId;

        /// <inheritdoc/>
        public String DataKind => _data.Kind;

        /// <summary> The item's bounding shape. </summary>
        [ExportGroup("Nodes")]
        [Export] private CollisionShape3D _collisionShape;

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private Sprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <summary> The item's data component. </summary>
        private ItemData _data;


        /// <summary> A reference to the item controller. </summary>
        private ItemController _itemController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;

            _itemController = ItemController.Instance;
        }


        /// <summary> When a unit enters the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyEntered(Node3D body)
        {
            if (body is ActorNode unit)
            {
                unit.AddUsableEntity(this);
            }
        }


        /// <summary> When a unit leaves the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is ActorNode unit)
            {
                unit.RemoveUsableEntity(this);
            }
        }


        /// <summary> Initialise the node with new data values. </summary>
        /// <param name="data"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        public void Initialise(ItemData data, Vector3 position)
        {
            _data = data;
            GlobalPosition = position;
            _sprite.Texture = data.GetInventorySprite();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            Double bobOffset = Math.Sin(Engine.GetFramesDrawn() * 0.01f) * 2f;
            _sprite.Offset = new Vector2(0f, (Single)bobOffset);

            _sprite.SortingOffset = (Int32)GlobalPosition.Y * 10000 + GlobalPosition.Z;
        }


        /// <inheritdoc/>
        public Vector3 GetWorldPosition() => GlobalPosition;


        /// <inheritdoc/>
        public CollisionShape3D GetCollisionShape() => _collisionShape;


        /// <inheritdoc/>
        public T GetData<T>() where T : EntityData
        {
            if (_data is T data)
            {
                return data;
            }
            else
            {
                throw new InvalidCastException($"Unable to cast the resource to {typeof(T)}.");
            }
        }


        /// <inheritdoc/>
        public void Examine(ActorNode activatingEntity) => _data.Examine(activatingEntity);


        /// <inheritdoc/>
        public void Use(ActorNode activatingEntity)
        {
            _data.Use(activatingEntity);

            // If the resource has been consumed.
            if (_data is FoodData foodResource && foodResource.CurrentPortions <= 0)
            {
                FreeObject();
            }
        }


        /// <summary> Attempt to grab the item and add it to the inventory. </summary>
        /// <param name="activatingEntity"> The unit attempting to grab the item. </param>
        public void Grab(ActorNode activatingEntity)
        {
            Boolean isSuccessful = activatingEntity.GetData<BeingData>().Inventory.TryAddItem(_data);
            if (isSuccessful)   // If the item was added, free it back to the pool.
            {
                FreeObject();
            }
        }


        /// <inheritdoc/>
        public void FreeObject() => _itemController.ItemPool.FreeObject(this);
    }
}
