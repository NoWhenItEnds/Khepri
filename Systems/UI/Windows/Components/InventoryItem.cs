using Godot;
using Khepri.Controllers;
using Khepri.Entities.Items;
using Khepri.Resources.Items;
using Khepri.Types;
using System;
using System.Linq;

namespace Khepri.UI.Windows.Components
{
    /// <summary> An inventory item in an inventory grid. </summary>
    public partial class InventoryItem : TextureButton, IPoolable<ItemResource>
    {
        /// <summary> A reference to the item's sprite. </summary>
        [ExportGroup("Nodes")]
        [Export] private AnimatedSprite2D _sprite;

        /// <summary> The position of the item's top-left position. </summary>
        public Vector2I CellPosition { get; private set; }


        /// <inheritdoc/>
        public ItemResource Resource { get; set; }


        /// <summary> A reference to the game world's viewport. </summary>
        private Viewport _viewport;

        /// <summary> A reference to the window this item is parented to. </summary>
        private InventoryWindow _window;

        /// <summary> A reference to the inventory this item is a part of. </summary>
        private EntityInventory _inventory;

        /// <summary> A reference to the object pool this object is a part of. </summary>
        private ObjectPool<InventoryItem, ItemResource> _objectPool;


        /// <summary> If the item it currently being held. </summary>
        private Boolean _isGrabbed = false;

        /// <summary> Whether the item is currently following the mouse. It will when it is clicked. </summary>
        private Boolean _isFollowingMouse = false;


        /// <inheritdoc/>
        public override void _Ready()
        {
            ButtonDown += OnButtonDown;
            ButtonUp += OnButtonUp;

            _viewport = GetViewport();
        }


        /// <summary> Create the inventory item by settings its internal variables. </summary>
        /// <param name="window"> A reference to the window this item is parented to. </param>
        /// <param name="resource"> The raw item data used to build the item. </param>
        /// <param name="cellPosition"> The position of the item's top-left position. </param>
        public void Initialise(InventoryWindow window, ItemResource resource, Vector2I cellPosition)
        {
            if (this is IPoolable<ItemResource> poolable)
            {
                poolable.Initialise(resource);
            }

            CellPosition = cellPosition;
            _window = window;
            _inventory = window.CurrentInventory;
            _objectPool = window.ItemPool;

            GlobalPosition = window.CalculatePosition(cellPosition);
            SetSprite(resource);
            Modulate = Colors.White;
            TextureClickMask = BuildClickMask(resource);
            Name = resource.Kind;
        }


        /// <summary> Pick the item up. </summary>
        private void OnButtonDown()
        {
            if (!_isGrabbed)
            {
                _isFollowingMouse = true;
                GrabItem();
            }
        }


        /// <summary> Pick the item up in the inventory. </summary>
        public void GrabItem()
        {
            _isGrabbed = true;
            _inventory.RemoveItem(Resource);
            Modulate = new Color(1, 1, 1, 0.5f);    // Make it transparent to easily see the object being grabbed.
        }


        /// <summary> Drop the item in the desired spot. </summary>
        private void OnButtonUp()
        {
            _isFollowingMouse = false;

            Vector2 mousePosition = _viewport.GetMousePosition();
            Vector2I cellPosition = _window.CalculatePosition(mousePosition);
            PlaceItem(cellPosition);
        }


        /// <summary> Return a grabbed item to the inventory. </summary>
        /// <param name="cellPosition"> The position to put the item. </param>
        public void PlaceItem(Vector2I cellPosition)
        {
            _isGrabbed = false;
            Modulate = Colors.White;  // Fix the object's transparency.

            Boolean isAdded = _inventory.TryAddItem(Resource, cellPosition);

            // If the item was added, update its position.
            if (isAdded)
            {
                CellPosition = cellPosition;
                GlobalPosition = _window.CalculatePosition(cellPosition);
            }
            else    // Snap it back to its previous position.
            {
                Boolean isReturned = _inventory.TryAddItem(Resource, CellPosition);
                if (isReturned)
                {
                    GlobalPosition = _window.CalculatePosition(CellPosition);   // Use the previously remembered cell position.
                }
                else    // If it couldn't be returned, for whatever reason, drop it.
                {
                    DropItem();
                }
            }
        }


        /// <summary> Drop the item at the player's feet. </summary>
        public void DropItem()
        {
            _inventory.RemoveItem(Resource);
            Vector3 playerPosition = PlayerController.Instance.PlayerUnit.GlobalPosition;
            ItemController.Instance.CreateItem(Resource, playerPosition);
            _window.RemoveItem(this);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (_isFollowingMouse)
            {
                Vector2 mousePosition = _viewport.GetMousePosition();
                GlobalPosition = mousePosition;
            }
        }


        /// <summary> Set the item's sprite based upon the information in the data object. </summary>
        /// <param name="resource"> The raw data to build the item. </param>
        /// <exception cref="ArgumentException"> If the data contains a type not mapped to the sprite sheet. </exception>
        private void SetSprite(ItemResource resource)
        {
            String itemType = resource.ItemType.ToString().Capitalize();
            String[] possibleTypes = _sprite.SpriteFrames.GetAnimationNames();
            if (!possibleTypes.Contains(itemType))
            {
                throw new ArgumentException("The map of inventory item sprites doesn't contain a category for the given type!");
            }

            Int32 itemIndex = Mathf.Clamp(resource.SpriteIndex, 0, _sprite.SpriteFrames.GetFrameCount(itemType) - 1);

            _sprite.Animation = itemType;
            _sprite.Frame = itemIndex;
            Size = resource.GetSize() * _window.CellSize;
        }


        /// <summary> Build the item's click mask based upon the inventory points. </summary>
        /// <param name="resource"> The raw data to build the item. </param>
        /// <returns> The constructed click mask. </returns>
        private Bitmap BuildClickMask(ItemResource resource)
        {
            Bitmap mask = new Bitmap();
            mask.Create((Vector2I)Size);

            Int32 cellSize = _window.CellSize;
            foreach (Vector2I point in resource.InventoryCells)
            {
                for (Int32 x = point.X * cellSize; x < (point.X + 1) * cellSize; x++)
                {
                    for (Int32 y = point.Y * cellSize; y < (point.Y + 1) * cellSize; y++)
                    {
                        if (x < Size.X && y < Size.Y)   // Guard to make sure we don't try to set beyond the mask (if the item's points are larger than the sprite used).
                        {
                            mask.SetBit(x, y, true);
                        }
                    }
                }
            }

            return mask;
        }


        /// <inheritdoc/>
        public void FreeObject() => _objectPool.FreeObject(this);
    }
}
