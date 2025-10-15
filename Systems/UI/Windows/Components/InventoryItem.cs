using Godot;
using Khepri.Controllers;
using Khepri.Entities.Items;
using Khepri.Resources;
using Khepri.Resources.Items;
using Khepri.Types;
using System;

namespace Khepri.UI.Windows.Components
{
    /// <summary> An inventory item in an inventory grid. </summary>
    public partial class InventoryItem : TextureButton, IPoolable
    {
        /// <summary> The position of the item's top-left position. </summary>
        public Vector2I CellPosition { get; private set; }

        /// <inheritdoc/>
        private ItemResource _resource;


        /// <summary> A reference to the window this item is parented to. </summary>
        private InventoryWindow _window;

        /// <summary> A reference to the inventory this item is a part of. </summary>
        private EntityInventory _inventory;


        /// <summary> If the item it currently being held. </summary>
        private Boolean _isGrabbed = false;


        /// <summary> Create the inventory item by settings its internal variables. </summary>
        /// <param name="window"> A reference to the window this item is parented to. </param>
        /// <param name="resource"> The raw item data used to build the item. </param>
        /// <param name="inventory"> The inventory the item is currently in. </param>
        /// <param name="cellPosition"> The position of the item's top-left position. </param>
        public void Initialise(InventoryWindow window, ItemResource resource, EntityInventory inventory, Vector2I cellPosition)
        {
            _resource = resource;
            CellPosition = cellPosition;
            _window = window;
            _inventory = inventory;

            GlobalPosition = window.CalculatePosition(cellPosition);
            SetSprite(resource);
            Modulate = Colors.White;
            TextureClickMask = BuildClickMask(resource);
            Name = resource.Id;
        }


        /// <summary> Pick the item up in the inventory. </summary>
        public void GrabItem()
        {
            _isGrabbed = true;
            _inventory.RemoveItem(_resource);
            Modulate = new Color(1, 1, 1, 0.5f);    // Make it transparent to easily see the object being grabbed.
        }


        /// <summary> Return a grabbed item to the inventory. </summary>
        /// <param name="cellPosition"> The position to put the item. </param>
        public void PlaceItem(Vector2I cellPosition)
        {
            _isGrabbed = false;
            Modulate = Colors.White;  // Fix the object's transparency.

            Boolean isAdded = _inventory.TryAddItem(_resource, cellPosition);

            // If the item was added, update its position.
            if (isAdded)
            {
                CellPosition = cellPosition;
                GlobalPosition = _window.CalculatePosition(cellPosition);
            }
            else    // Snap it back to its previous position.
            {
                Boolean isReturned = _inventory.TryAddItem(_resource, CellPosition);
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
            _inventory.RemoveItem(_resource);
            Vector3 playerPosition = PlayerController.Instance.PlayerBeing.GlobalPosition;
            ItemController.Instance.CreateItem(_resource, playerPosition);
            FreeObject();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (_isGrabbed) // Move the item to follow the current selection.
            {
                GlobalPosition = _window.SelectionPosition;
            }
        }


        /// <summary> Set the item's sprite based upon the information in the data object. </summary>
        /// <param name="resource"> The raw data to build the item. </param>
        private void SetSprite(ItemResource resource)
        {
            TextureNormal = resource.InventorySprite;
            Size = resource.GetSize() * InventoryWindow.CELL_SIZE;
        }


        /// <summary> Build the item's click mask based upon the inventory points. </summary>
        /// <param name="resource"> The raw data to build the item. </param>
        /// <returns> The constructed click mask. </returns>
        private Bitmap BuildClickMask(ItemResource resource)
        {
            Bitmap mask = new Bitmap();
            mask.Create((Vector2I)Size);

            Int32 cellSize = InventoryWindow.CELL_SIZE;
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
        public T GetResource<T>() where T : EntityResource
        {
            if (_resource is T resource)
            {
                return resource;
            }
            else
            {
                throw new InvalidCastException($"Unable to cast the resource to {typeof(T)}.");
            }
        }


        /// <inheritdoc/>
        public void FreeObject() => _window.ItemPool.FreeObject(this);
    }
}
