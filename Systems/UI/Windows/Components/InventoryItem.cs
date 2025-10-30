using Godot;
using Khepri.Data;
using Khepri.Data.Items;
using Khepri.Types;
using Khepri.Types.Exceptions;
using System;

namespace Khepri.UI.Windows.Components
{
    /// <summary> An inventory item in an inventory grid. </summary>
    public partial class InventoryItem : TextureButton, IPoolable
    {
        /// <summary> An event that is triggered when the item is interacted with. </summary>
        public event Action<InventoryItem> ItemPressed;


        /// <summary> The position of the item's top-left position. </summary>
        public Vector2I CellPosition { get; private set; }


        /// <summary> The item's data component. </summary>
        private ItemData _data;

        /// <summary> A reference to the item pool the entity is a part of. </summary>
        private ObjectPool<InventoryItem> _itemPool;

        /// <summary> A reference to the inventory grid this item represents. </summary>
        /// <remarks> A null means that it is dangling. </remarks>
        private InventoryGrid? _grid = null;


        /// <summary> Spawn the item and set its initial values. </summary>
        /// <param name="itemPool"> A reference to the item pool the entity is a part of. </param>
        public void Spawn(ObjectPool<InventoryItem> itemPool)
        {
            _itemPool = itemPool;
            ButtonDown += OnButtonDown;
        }


        /// <summary> When the button is interacted with, let the window know. </summary>
        private void OnButtonDown() => ItemPressed?.Invoke(this);


        /// <summary> Create the inventory item by settings its internal variables. </summary>
        /// <param name="data"> The raw item data used to build the item. </param>
        /// <param name="grid"> A reference to the inventory grid this item represents. </param>
        /// <param name="cellPosition"> The position of the item's top-left position. </param>
        public void Initialise(InventoryGrid grid, ItemData data, Vector2I cellPosition)
        {
            SetGrid(grid);
            _data = data;
            CellPosition = cellPosition;

            GlobalPosition = grid.CalculatePosition(cellPosition);
            SetSprite(data);
            Modulate = Colors.White;
            TextureClickMask = BuildClickMask(data);
            Name = data.Kind;
        }


        /// <summary> Set which grid the item represents. </summary>
        /// <param name="grid"> A reference to the inventory grid this item represents. </param>
        public void SetGrid(InventoryGrid grid)
        {
            _grid = grid;
        }


        /// <summary> Pick the item up in the inventory. </summary>
        /// <exception cref="UIException"> If the set grid is invalid. </exception>
        public void GrabItem()
        {
            if (_grid == null || _grid.Inventory == null)
            {
                throw new UIException("When calling this method, it should have a reference to a grid and an inventory.");
            }

            _grid.Inventory.RemoveItem(_data);
            Modulate = new Color(1, 1, 1, 0.5f);    // Make it transparent to easily see the object being grabbed.
            _grid = null;   // As the item has been removed from the inventory, it no longer has a grid.
        }


        /// <summary> Attempt to return a grabbed item to AN inventory. </summary>
        /// <param name="grid"> A reference to the inventory grid to add this item to. </param>
        /// <param name="cellPosition"> The position to put the item. </param>
        /// <returns> Whether the item was added to the inventory. </returns>
        /// <exception cref="UIException"> If the given grid is invalid. </exception>
        public Boolean TryPlaceItem(InventoryGrid grid, Vector2I cellPosition)
        {
            if (grid.Inventory == null)
            {
                throw new UIException("When calling this method, the given grid should have a reference to an inventory.");
            }

            Boolean isAdded = grid.Inventory.TryAddItem(_data, cellPosition);

            // If the item was added, update its position.
            if (isAdded)
            {
                _grid = grid;
                Modulate = Colors.White;  // Fix the object's transparency.
                CellPosition = cellPosition;
                GlobalPosition = _grid.CalculatePosition(cellPosition);
            }
            return isAdded;
        }


        /// <summary> Set the item's sprite based upon the information in the data object. </summary>
        /// <param name="resource"> The raw data to build the item. </param>
        private void SetSprite(ItemData resource)
        {
            TextureNormal = resource.GetInventorySprite();
            Size = resource.GetSize() * InventoryGrid.CELL_SIZE;
        }


        /// <summary> Build the item's click mask based upon the inventory points. </summary>
        /// <param name="resource"> The raw data to build the item. </param>
        /// <returns> The constructed click mask. </returns>
        private Bitmap BuildClickMask(ItemData resource)
        {
            Bitmap mask = new Bitmap();
            mask.Create((Vector2I)Size);

            Int32 cellSize = InventoryGrid.CELL_SIZE;
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
        public void FreeObject() => _itemPool.FreeObject(this);
    }
}
