using Godot;
using Khepri.Entities.Actors;
using Khepri.Resources.Items;
using Khepri.Types;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A node representing an item in the game world. </summary>
    public partial class ItemNode : StaticBody3D, IEntity, IPoolable<ItemResource>
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private Sprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <inheritdoc/>
        [ExportGroup("Statistics")]
        [Export] public ItemResource Resource { get; set; }

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


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
            if (body is Unit unit)
            {
                unit.AddUsableEntity(this);
            }
        }


        /// <summary> When a unit leaves the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is Unit unit)
            {
                unit.RemoveUsableEntity(this);
            }
        }


        /// <summary> Initialise the node with new data values. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        public void Initialise(ItemResource resource, Vector3 position)
        {
            if (this is IPoolable<ItemResource> poolable)
            {
                poolable.Initialise(resource);
            }
            GlobalPosition = position;
            _sprite.Texture = resource.WorldSprite;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            Double bobOffset = Math.Sin(Engine.GetFramesDrawn() * 0.01f) * 2f;
            _sprite.Offset = new Vector2(0f, (Single)bobOffset);

            _sprite.SortingOffset = (Int32)GlobalPosition.Y * 10000 + GlobalPosition.Z;
        }


        /// <inheritdoc/>
        public void Examine(Unit activatingEntity) => Resource.Examine(activatingEntity);


        /// <inheritdoc/>
        public void Use(Unit activatingEntity)
        {
            Resource.Use(activatingEntity);

            // If the resource has been consumed.
            if (Resource is FoodResource foodResource && foodResource.Portions <= 0)
            {
                FreeObject();
            }
        }


        /// <summary> Attempt to grab the item and add it to the inventory. </summary>
        /// <param name="activatingEntity"> The unit attempting to grab the item. </param>
        public void Grab(Unit activatingEntity)
        {
            Boolean isSuccessful = activatingEntity.Inventory.TryAddItem(Resource);
            if (isSuccessful)   // If the item was added, free it back to the pool.
            {
                FreeObject();
            }
        }


        /// <inheritdoc/>
        public void FreeObject() => _itemController.ItemPool.FreeObject(this);
    }
}
