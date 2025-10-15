using Godot;
using Khepri.Entities.Items;
using Khepri.Resources.Items;
using Khepri.Types;
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


        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<InventoryItem> ItemPool { get; private set; }

        /// <summary> Get the current selection's position on the screen. </summary>
        public Vector2 SelectionPosition => CalculatePosition(_currentSelection);


        /// <summary> A reference to the origin inventory. </summary>
        private EntityInventory _originInventory;

        /// <summary> A reference to the targeted inventory. </summary>
        private EntityInventory? _targetInventory = null;

        /// <summary> The number of cells in the inventory. </summary>
        private Vector2I _gridSize = Vector2I.One;

        /// <summary> The currently selected grid cell. </summary>
        private Vector2I _currentSelection = Vector2I.Zero;

        /// <summary> A reference to the currently grabbed item. </summary>
        private InventoryItem? _currentHeldItem = null;


        /// <summary> The size of the cell in pixels. </summary>
        public const Int32 CELL_SIZE = 32;


        /// <inheritdoc/>
        public override void _Ready()
        {
            ItemPool = new ObjectPool<InventoryItem>(this, _inventoryItemPrefab);
            foreach (InventoryItem item in ItemPool.GetAllObjects())
            {
                item.ButtonDown += PickupItem;
            }

            GetViewport().SizeChanged += OnSizeChanged;
            VisibilityChanged += OnVisibilityChanged;
        }


        /// <summary> Update the window when the size changes. </summary>
        private void OnSizeChanged()
        {
            InventoryItem[] activeItems = ItemPool.GetActiveObjects();
            foreach (InventoryItem item in activeItems)
            {
                item.GlobalPosition = CalculatePosition(item.CellPosition);
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
                ItemPool.FreeAll();
            }
        }


        /// <summary> Initialise the window by referencing an entity's inventory. </summary>
        /// <param name="originInventory"> The controlled entity's inventory. </param>
        public void Initialise(EntityInventory originInventory)
        {
            _originInventory = originInventory;
            _currentSelection = Vector2I.Zero;

            // Set inventory size.
            _gridSize = originInventory.InventorySize;
            Single halfSize = CELL_SIZE * 0.5f;
            _inventoryGrid.OffsetTop = -_gridSize.X * halfSize;
            _inventoryGrid.OffsetBottom = _gridSize.X * halfSize;
            _inventoryGrid.OffsetLeft = -_gridSize.Y * halfSize;
            _inventoryGrid.OffsetRight = _gridSize.Y * halfSize;

            // Get the items. We need to do this as a 'unique' check as a single item takes up multiple slots.
            Dictionary<ItemResource, Vector2I> items = new Dictionary<ItemResource, Vector2I>();
            for (Int32 x = 0; x < originInventory.InventorySize.X; x++)
            {
                for (Int32 y = 0; y < originInventory.InventorySize.Y; y++)
                {
                    ItemResource? item = originInventory.GetItem(x, y);
                    if (item != null)
                    {
                        items.TryAdd(item, new Vector2I(x, y));
                    }
                }
            }

            foreach (KeyValuePair<ItemResource, Vector2I> item in items)
            {
                CreateItem(item.Key, item.Value);
            }
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="resource"> The data to initialise the item with. </param>
        /// <param name="position"> The cell position to create the object at. This is the object's top-left corner. </param>
        /// <returns> The initialised item. </returns>
        private InventoryItem CreateItem(ItemResource resource, Vector2I position)
        {
            InventoryItem item = ItemPool.GetAvailableObject();
            item.Initialise(this, resource, _originInventory, position);
            return item;
        }


        /// <summary> Calculate the screen position of a given cell coordinate. </summary>
        /// <param name="position"> The cell coordinate. </param>
        /// <returns> The screen position of the cell's top left corner.</returns>
        public Vector2 CalculatePosition(Vector2I position)
        {
            Vector2 clampedPosition = new Vector2(Mathf.Clamp(position.X, 0, _gridSize.X - 1), Mathf.Clamp(position.Y, 0, _gridSize.Y - 1));
            return _inventoryGrid.GlobalPosition + clampedPosition * CELL_SIZE;
        }


        /// <summary> Calculate the grid coordinate a given screen position. </summary>
        /// <param name="position"> The screen coordinate. </param>
        /// <returns> The cell coordinate of the cell's top left corner.</returns>
        public Vector2I CalculatePosition(Vector2 position)
        {
            Vector2 relativePosition = (position - _inventoryGrid.GlobalPosition) / CELL_SIZE;
            return new Vector2I((Int32)Mathf.Clamp(relativePosition.X, 0, _gridSize.X - 1), (Int32)Mathf.Clamp(relativePosition.Y, 0, _gridSize.Y - 1));
        }


        /// <summary> Dispose of an item, returning it back to the item pool. </summary>
        /// <param name="item"> The item to dispose. </param>
        public void RemoveItem(InventoryItem item) => ItemPool.FreeObject(item);


        /// <summary> Get a given inventory item at a given position. </summary>
        /// <param name="position"> The cell position to check. </param>
        /// <returns> The returned inventory item. Null means that there wasn't one at this position. </returns>
        private InventoryItem? GetInventoryItem(Vector2I position)
        {
            ItemResource? item = _originInventory.GetItem(position);
            if (item == null)
            {
                return null;
            }

            return ItemPool.GetActiveObjects().Where(x => x.GetResource<ItemResource>() == item).FirstOrDefault() ?? null;
        }


        /// <inheritdoc/>
        public override void _Input(InputEvent @event)
        {
            if (Visible)
            {
                if (@event is InputEventMouseMotion mouseEvent) // Allow the mouse to move / override the current selection.
                {
                    _currentSelection = CalculatePosition(mouseEvent.GlobalPosition);
                }
                else
                {
                    if (Input.IsActionJustPressed("action_ui_accept"))
                    {
                        PickupItem();
                    }
                    else if (@event.IsActionReleased("action_ui_cancel"))
                    {
                        DropItem();
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
                            Vector2I itemSize = item.GetResource<ItemResource>().GetSize();  // We only need to apply the size if we're going 'forwards', not 'backwards' as the position for a large object will be the top-left.
                            _currentSelection += direction * new Vector2I(direction.X > 0 ? itemSize.X : 1, direction.Y > 0 ? itemSize.Y : 1);
                        }
                        else
                        {
                            _currentSelection += direction;
                        }
                        _currentSelection = _currentSelection.Clamp(Vector2I.Zero, _gridSize - Vector2I.One);
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


        /// <summary> Pickup / place the currently selected item. </summary>
        private void PickupItem()
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


        /// <summary> Drop / cancel the grab of the currently held item. </summary>
        private void DropItem()
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


        /// <inheritdoc/>
        public override void _Draw()
        {
            if (Visible)
            {
                // This relies upon the sort order of this and the grid texture being manually set.
                Vector2 start = _inventoryGrid.Position + (_currentSelection * CELL_SIZE);

                Vector2 size = Vector2.One * CELL_SIZE;
                ItemResource? currentItem = _originInventory.GetItem(_currentSelection);
                if (currentItem != null)
                {
                    size = currentItem.GetSize() * CELL_SIZE;
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


        /// <inheritdoc/>
        public override void _ExitTree()
        {
            foreach (InventoryItem item in ItemPool.GetAllObjects())
            {
                item.ButtonUp -= PickupItem;
            }
        }
    }
}
