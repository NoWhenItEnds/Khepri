using Godot;
using Khepri.Entities.Components;
using Khepri.Entities.Items;
using Khepri.UI.Windows.Components;
using System;
using System.Collections.Generic;
using System.Linq;

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


        /// <summary> The size of the cell in pixels. </summary>
        [ExportGroup("Settings")]
        [Export] public Int32 CellSize { get; private set; } = 32;

        /// <summary> How many item nodes the item pool should contain. </summary>
        [Export] private Int32 _poolSize = 100;


        /// <summary> A pool that holds items that are in the game world. A boolean shows if an object is currently in use. </summary>
        private Dictionary<InventoryItem, Boolean> _itemPool = new Dictionary<InventoryItem, Boolean>();

        /// <summary> A reference to the currently open inventory. </summary>
        private EntityInventory _currentInventory;

        /// <summary> The number of cells in the inventory. </summary>
        private Vector2I _gridSize = Vector2I.One;

        /// <summary> The currently selected grid cell. </summary>
        private Vector2I _currentSelection = Vector2I.Zero;

        /// <summary> A reference to the currently grabbed item. </summary>
        private InventoryItem? _currentHeldItem = null;


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
            //  Return a held item back to its position.
            if (_currentHeldItem != null)
            {
                _currentHeldItem.PlaceItem(_currentHeldItem.CellPosition);
                _currentHeldItem = null;
            }

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
            _currentInventory = inventory;
            _currentSelection = Vector2I.Zero;

            // Set inventory size.
            _gridSize = inventory.InventorySize;
            Single halfSize = CellSize * 0.5f;
            _inventoryGrid.OffsetTop = -_gridSize.X * halfSize;
            _inventoryGrid.OffsetBottom = _gridSize.X * halfSize;
            _inventoryGrid.OffsetLeft = -_gridSize.Y * halfSize;
            _inventoryGrid.OffsetRight = _gridSize.Y * halfSize;

            // Get the unique items.
            Dictionary<ItemData, Vector2I> items = new Dictionary<ItemData, Vector2I>();
            for (Int32 x = 0; x < inventory.InventorySize.X; x++)
            {
                for (Int32 y = 0; y < inventory.InventorySize.Y; y++)
                {
                    ItemData? data = inventory.GetItem(x, y);
                    if (data != null)
                    {
                        items.TryAdd(data, new Vector2I(x, y));
                    }
                }
            }

            foreach (KeyValuePair<ItemData, Vector2I> item in items)
            {
                CreateItem(item.Key, item.Value, inventory);
            }
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="data"> The data to initialise the item with. </param>
        /// <param name="position"> The cell position to create the object at. This is the object's top-left corner. </param>
        /// <param name="inventory"> A reference to the inventory this item is a part of. </param>
        /// <returns> The initialise item. </returns>
        private InventoryItem CreateItem(ItemData data, Vector2I position, EntityInventory inventory)
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
            result.Modulate = Colors.White;

            return result;
        }


        /// <summary> Calculate the screen position of a given cell coordinate. </summary>
        /// <param name="position"> The cell coordinate. </param>
        /// <returns> The screen position of the cell's top left corner.</returns>
        public Vector2 CalculatePosition(Vector2I position)
        {
            Vector2 clampedPosition = new Vector2(Mathf.Clamp(position.X, 0, _gridSize.X - 1), Mathf.Clamp(position.Y, 0, _gridSize.Y - 1));
            return _inventoryGrid.GlobalPosition + clampedPosition * CellSize;
        }


        /// <summary> Calculate the grid coordinate a given screen position. </summary>
        /// <param name="position"> The screen coordinate. </param>
        /// <returns> The cell coordinate of the cell's top left corner.</returns>
        public Vector2I CalculatePosition(Vector2 position)
        {
            Vector2 relativePosition = (position - _inventoryGrid.GlobalPosition) / CellSize;
            return new Vector2I((Int32)Mathf.Clamp(relativePosition.X, 0, _gridSize.X - 1), (Int32)Mathf.Clamp(relativePosition.Y, 0, _gridSize.Y - 1));
        }


        /// <summary> Free an item and return it to the pool. </summary>
        /// <param name="item"> The item to return. </param>
        public void FreeItem(InventoryItem item)
        {
            _itemPool[item] = false;
            item.GlobalPosition = new Vector2(-100f, -100f);
            item.Visible = false;
        }


        /// <summary> Get a given inventory item at a given position. </summary>
        /// <param name="position"> The cell position to check. </param>
        /// <returns> The returned inventory item. Null means that there wasn't one at this position. </returns>
        private InventoryItem? GetInventoryItem(Vector2I position)
        {
            ItemData? item = _currentInventory.GetItem(position);
            if (item == null)
            {
                return null;
            }

            return _itemPool.Where(x => x.Value && x.Key.Data == item).Select(x => x.Key).FirstOrDefault() ?? null;
        }


        /// <inheritdoc/>
        public override void _Input(InputEvent @event)
        {
            if (Visible)
            {
                if (@event is InputEventMouseMotion mouseEvent)
                {
                    _currentSelection = CalculatePosition(mouseEvent.GlobalPosition);
                }
                else
                {
                    // Handle non-mouse input. Stop all input if the mouse if being used.
                    if (!Input.IsMouseButtonPressed(MouseButton.Left) && !Input.IsMouseButtonPressed(MouseButton.Right) && @event is not InputEventMouseMotion)
                    {
                        // Move the item.
                        if (Input.IsActionJustPressed("action_ui_accept"))
                        {
                            if (_currentHeldItem == null)
                            {
                                _currentHeldItem = GetInventoryItem(_currentSelection);
                                if (_currentHeldItem != null)
                                {
                                    _currentHeldItem.GrabItem();
                                }
                            }
                            else
                            {
                                _currentHeldItem.PlaceItem(_currentSelection);
                                _currentHeldItem = null;
                            }
                        }

                        // Drop the item
                        if (@event.IsActionReleased("action_ui_cancel"))
                        {
                            if (_currentHeldItem != null)   // Return the item back to its previous position in the inventory.
                            {
                                _currentHeldItem.PlaceItem(_currentHeldItem.CellPosition);
                                _currentHeldItem = null;
                            }
                            else                            // Drop the item from the inventory.
                            {
                                InventoryItem? item = GetInventoryItem(_currentSelection);
                                if (item != null)
                                {
                                    item.DropItem();
                                }
                            }
                        }

                        // Move the cursor.
                        Int32 moveHorizontal =
                            (@event.IsActionPressed("action_ui_left") ? -1 : 0) +
                            (@event.IsActionPressed("action_ui_right") ? 1 : 0);
                        Int32 moveVertical =
                            (@event.IsActionPressed("action_ui_up") ? -1 : 0) +
                            (@event.IsActionPressed("action_ui_down") ? 1 : 0);
                        Vector2I direction = new Vector2I(moveHorizontal, moveVertical);
                        if (direction != Vector2I.Zero)
                        {
                            // Check how far to move from the current position.
                            InventoryItem? item = GetInventoryItem(_currentSelection);
                            if (item != null)
                            {
                                Vector2I itemSize = item.Data.GetSize();  // We only need to apply the size if we're going 'forwards', not 'backwards' as the position for a large object will be the top-left.
                                _currentSelection += direction * new Vector2I(direction.X > 0 ? itemSize.X : 1, direction.Y > 0 ? itemSize.Y : 1);
                            }
                            else
                            {
                                _currentSelection += direction;
                            }
                            _currentSelection = _currentSelection.Clamp(Vector2I.Zero, _gridSize - Vector2I.One);
                        }
                    }
                }

                // Check if we've landed on an object, and need to move to its top left corner.
                InventoryItem? currentItem = GetInventoryItem(_currentSelection);
                if (currentItem != null)
                {
                    _currentSelection = currentItem.CellPosition;
                }
                QueueRedraw();
            }
        }


        /// <inheritdoc/>
        public override void _Draw()
        {
            if (Visible)
            {
                // This relies upon the sort order of this and the grid texture being manually set.
                Vector2 start = _inventoryGrid.Position + (_currentSelection * CellSize);

                Vector2 size = Vector2.One * CellSize;
                ItemData? currentItem = _currentInventory.GetItem(_currentSelection);
                if (currentItem != null)
                {
                    size = currentItem.GetSize() * CellSize;
                }

                Rect2 selectionRect = new Rect2(start.X, start.Y, size.X, size.Y);
                DrawRect(selectionRect, Colors.BlueViolet, false, 8f);
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // If we have a held object, update its position.
            if (_currentHeldItem != null)
            {
                _currentHeldItem.GlobalPosition = CalculatePosition(_currentSelection);
            }
        }
    }
}
