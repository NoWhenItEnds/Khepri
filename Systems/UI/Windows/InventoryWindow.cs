using Godot;
using Khepri.Entities.Components;
using Khepri.Entities.Items.Components;
using Khepri.UI.Windows.Components;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Windows
{
    /// <summary> A window allowing access to an entity's inventory. </summary>
    public partial class InventoryWindow : Control
    {
        /// <summary> The prefab to use when spawning item buttons. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _inventoryItemPrefab;

        /// <summary> A reference to the grid texture. </summary>
        [ExportGroup("Nodes")]
        [Export] private TextureRect _inventoryGrid;


        /// <summary> The number of cells wide the inventory is. </summary>
        [ExportGroup("Settings")]
        [Export] public Int32 GridWidth { get; private set; } = 10;

        /// <summary> The number of cells high the inventory is. </summary>
        [Export] public Int32 GridHeight { get; private set; } = 10;

        /// <summary> The size of the cell in pixels. </summary>
        [Export] public Int32 CellSize { get; private set; } = 32;

        /// <summary> How many item nodes the item pool should contain. </summary>
        [Export] private Int32 _poolSize = 100;


        /// <summary> A pool that holds items that are in the game world. A boolean shows if an object is currently in use. </summary>
        private Dictionary<InventoryItem, Boolean> _itemPool = new Dictionary<InventoryItem, Boolean>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            GetViewport().SizeChanged += OnSizeChanged;
            VisibilityChanged += OnVisibilityChanged;
            OnSizeChanged();

            // Build the pool.
            for (Int32 i = 0; i < _poolSize - _itemPool.Count; i++)
            {
                InventoryItem item = BuildItem();
                _itemPool.Add(item, false);
            }
        }


        /// <summary> Update the window when the size changes. </summary>
        private void OnSizeChanged()
        {
            foreach (KeyValuePair<InventoryItem, Boolean> item in _itemPool)
            {
                if (item.Value) // If the item is currently in use.
                {
                    item.Key.GlobalPosition = CalculatePosition(item.Key.CellPosition);
                }
            }
        }


        /// <summary> Clean the window up if the window is hidden. </summary>
        private void OnVisibilityChanged()
        {
            // If the window has been hidden, clean up.
            if (!Visible)
            {
                foreach (KeyValuePair<InventoryItem, Boolean> item in _itemPool)
                {
                    if (item.Value)
                    {
                        FreeItem(item.Key);
                    }
                }
            }
        }


        /// <summary> Create a new item for the pool. </summary>
        /// <returns> The created item node. </returns>
        private InventoryItem BuildItem()
        {
            InventoryItem item = _inventoryItemPrefab.Instantiate<InventoryItem>();
            AddChild(item);
            item.GlobalPosition = new Vector2(-100f, -100f);
            item.Visible = false;
            return item;
        }


        /// <summary> Initialise the window by referencing an entity's inventory. </summary>
        /// <param name="inventory"> The entity's inventory. </param>
        public void Initialise(EntityInventory inventory)
        {
            // Get the unique items.
            Dictionary<ItemDataComponent, Vector2I> items = new Dictionary<ItemDataComponent, Vector2I>();
            for (Int32 x = 0; x < inventory.StoredItems.GetLength(0); x++)
            {
                for (Int32 y = 0; y < inventory.StoredItems.GetLength(1); y++)
                {
                    if (inventory.StoredItems[x, y] != null)
                    {
                        items.Add(inventory.StoredItems[x, y], new Vector2I(x, y));
                    }
                }
            }

            foreach (KeyValuePair<ItemDataComponent, Vector2I> item in items)
            {
                CreateItem(item.Key, item.Value, inventory);
            }
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="data"> The data to initialise the item with. </param>
        /// <param name="position"> The cell position to create the object at. This is the object's top-left corner. </param>
        /// <param name="inventory"> A reference to the inventory this item is a part of. </param>
        /// <returns> The initialise item. </returns>
        private InventoryItem CreateItem(ItemDataComponent data, Vector2I position, EntityInventory inventory)
        {
            InventoryItem result = null;

            // Attempt to find a free item in the pool.
            foreach (KeyValuePair<InventoryItem, Boolean> item in _itemPool)
            {
                if (!item.Value)    // If the item is free.
                {
                    result = item.Key;
                    break;
                }
            }

            // If an item was not found, expand the pool.
            if (result == null)
            {
                result = BuildItem();
                _poolSize++;
            }

            _itemPool[result] = true;
            result.Initialise(data, position, inventory, this);
            result.GlobalPosition = CalculatePosition(position);
            result.Visible = true;

            return result;
        }


        /// <summary> Calculate the screen position of a given cell coordinate. </summary>
        /// <param name="position"> The cell coordinate. </param>
        /// <returns> The screen position of the cell's top left corner.</returns>
        public Vector2 CalculatePosition(Vector2I position)
        {
            Vector2 clampedPosition = new Vector2(Mathf.Clamp(position.X, 0, GridWidth - 1), Mathf.Clamp(position.Y, 0, GridHeight - 1));
            return _inventoryGrid.GlobalPosition + clampedPosition * CellSize;
        }


        /// <summary> Calculate the grid coordinate a given screen position. </summary>
        /// <param name="position"> The screen coordinate. </param>
        /// <returns> The cell coordinate of the cell's top left corner.</returns>
        public Vector2I CalculatePosition(Vector2 position)
        {
            Vector2 relativePosition = (position - _inventoryGrid.GlobalPosition) / CellSize;
            return new Vector2I((Int32)Mathf.Clamp(relativePosition.X, 0, GridWidth - 1), (Int32)Mathf.Clamp(relativePosition.Y, 0, GridHeight - 1));
        }


        /// <summary> Free an item and return it to the pool. </summary>
        /// <param name="item"> The item to return. </param>
        private void FreeItem(InventoryItem item)
        {
            _itemPool[item] = false;
            item.GlobalPosition = new Vector2(-100f, -100f);
            item.Visible = false;
        }
    }
}
