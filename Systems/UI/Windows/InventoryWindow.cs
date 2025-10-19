using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using Khepri.Resources.Items;
using Khepri.Types;
using Khepri.Types.Exceptions;
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

        /// <summary> A reference to the player's inventory grid. </summary>
        [ExportGroup("Nodes")]
        [Export] private InventoryGrid _inventoryGrid;


        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<InventoryItem> ItemPool { get; private set; }

        /// <summary> Get the current selection's position on the screen. </summary>
        public Vector2 SelectionPosition => _inventoryGrid.CalculatePosition(_currentSelection);


        /// <summary> The currently selected grid cell. </summary>
        private Vector2I _currentSelection = Vector2I.Zero;

        /// <summary> A reference to the currently grabbed item. </summary>
        private InventoryItem? _currentHeldItem = null;


        /// <inheritdoc/>
        public override void _Ready()
        {
            ItemPool = new ObjectPool<InventoryItem>(this, _inventoryItemPrefab);
            foreach (InventoryItem item in ItemPool.GetAllObjects())
            {
                item.Spawn(ItemPool);
                item.ItemPressed += PickupItem;
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
                item.GlobalPosition = _inventoryGrid.CalculatePosition(item.CellPosition);
            }
        }


        /// <summary> Clean the window up if the window is hidden. </summary>
        private void OnVisibilityChanged()
        {
            //  Return a held item back to its position.
            if (_currentHeldItem != null)
            {
                Boolean isAdded = _currentHeldItem.TryPlaceItem(_inventoryGrid, _currentHeldItem.CellPosition);
                if(!isAdded) { DropItem(_currentHeldItem); }
                _currentHeldItem = null;
            }

            // If the window has been hidden, clean up.
            if (!Visible)
            {
                ItemPool.FreeAll();
            }
        }


        /// <summary> Initialise the window by referencing an entity's inventory. </summary>
        /// <param name="playerInventory"> The controlled entity's inventory. </param>
        public void Initialise(EntityInventory playerInventory)
        {
            _inventoryGrid.Initialise(playerInventory, ItemPool);
            _currentSelection = Vector2I.Zero;

            // Get the items. We need to do this as a 'unique' check as a single item takes up multiple slots.
            Dictionary<ItemResource, Vector2I> items = playerInventory.GetItems();
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
            if(_inventoryGrid == null || _inventoryGrid.Inventory == null)
            {
                throw new UIException("The inventory should have been initialised before calling this method.");
            }

            InventoryItem item = ItemPool.GetAvailableObject();
            item.Initialise(_inventoryGrid, resource, position);
            return item;
        }


        /// <summary> Dispose of an item, returning it back to the item pool. </summary>
        /// <param name="item"> The item to dispose. </param>
        public void RemoveItem(InventoryItem item) => ItemPool.FreeObject(item);


        /// <summary> Calculate the screen position of a given cell coordinate. </summary>
        /// <param name="position"> The cell coordinate. </param>
        /// <returns> The screen position of the cell's top left corner.</returns>
        public Vector2 CalculatePosition(Vector2I position) => _inventoryGrid.CalculatePosition(position);


        /// <summary> Calculate the grid coordinate a given screen position. </summary>
        /// <param name="position"> The screen coordinate. </param>
        /// <returns> The cell coordinate of the cell's top left corner.</returns>
        public Vector2I CalculatePosition(Vector2 position) => _inventoryGrid.CalculatePosition(position);


        /// <inheritdoc/>
        public override void _Input(InputEvent @event)
        {
            if (Visible)
            {
                if (@event is InputEventMouseMotion mouseEvent) // Allow the mouse to move / override the current selection.
                {
                    _currentSelection = _inventoryGrid.CalculatePosition(mouseEvent.GlobalPosition);
                }
                else
                {
                    if (Input.IsActionJustPressed("action_ui_accept"))
                    {
                        PickupItem(_inventoryGrid.GetInventoryItem(_currentSelection));
                    }
                    else if (@event.IsActionReleased("action_ui_cancel"))
                    {
                        DropItem(_inventoryGrid.GetInventoryItem(_currentSelection));
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
                        InventoryItem? item = _inventoryGrid.GetInventoryItem(_currentSelection);
                        if (item != null)
                        {
                            Vector2I itemSize = item.GetResource<ItemResource>().GetSize();  // We only need to apply the size if we're going 'forwards', not 'backwards' as the position for a large object will be the top-left.
                            _currentSelection += direction * new Vector2I(direction.X > 0 ? itemSize.X : 1, direction.Y > 0 ? itemSize.Y : 1);
                        }
                        else
                        {
                            _currentSelection += direction;
                        }
                        _currentSelection = _currentSelection.Clamp(Vector2I.Zero, _inventoryGrid.GridSize - Vector2I.One);
                    }
                }

                // Check if we've landed on an object, and need to move to its top left corner.
                InventoryItem? currentItem = _inventoryGrid.GetInventoryItem(_currentSelection);
                if (currentItem != null)
                {
                    _currentSelection = currentItem.CellPosition;
                }
                QueueRedraw();
            }
        }


        /// <summary> Pickup / place the given item. </summary>
        /// <param name="item"> The item to manipulate, or null if an empty position was selected. </param>
        private void PickupItem(InventoryItem? item)
        {
            if (_currentHeldItem == null)
            {
                if (item != null)
                {
                    _currentHeldItem = item;
                    _currentHeldItem.GrabItem();
                }
            }
            else
            {
                Boolean isAdded = _currentHeldItem.TryPlaceItem(_inventoryGrid, _currentSelection);
                if (!isAdded) { DropItem(_currentHeldItem); }
                _currentHeldItem = null;
            }
        }


        /// <summary> Drop / cancel the grab of the given item. </summary>
        /// <param name="item"> The item to manipulate, or null if an empty position was selected. </param>
        private void DropItem(InventoryItem? item)
        {
            if(item != null)
            {
                Vector3 playerPosition = ActorController.Instance.GetPlayer().GlobalPosition;
                ItemController.Instance.CreateItem(item.GetResource<ItemResource>(), playerPosition);
                item.FreeObject();
            }
        }


        /// <inheritdoc/>
        public override void _Draw()
        {
            if (Visible)
            {
                Int32 cellSize = InventoryGrid.CELL_SIZE;

                // This relies upon the sort order of this and the grid texture being manually set.
                Vector2 start = _inventoryGrid.Position + (_currentSelection * cellSize);

                Vector2 size = Vector2.One * cellSize;
                ItemResource? currentItem = _inventoryGrid.Inventory?.GetItem(_currentSelection);
                if (currentItem != null)
                {
                    size = currentItem.GetSize() * cellSize;
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
                _currentHeldItem.GlobalPosition = _inventoryGrid.CalculatePosition(_currentSelection);
            }
        }


        /// <inheritdoc/>
        public override void _ExitTree()
        {
            foreach (InventoryItem item in ItemPool.GetAllObjects())
            {
                item.ItemPressed -= PickupItem;
            }
        }
    }
}
