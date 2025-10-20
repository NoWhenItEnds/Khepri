using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using Khepri.Resources.Items;
using Khepri.Types;
using Khepri.UI.Windows.Components;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Windows
{
    /// <summary> The window used for transferring items from one inventory to another. </summary>
    public partial class StorageTransferWindow : Control
    {
        /// <summary> The prefab to use when spawning item buttons. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _inventoryItemPrefab;

        /// <summary> A reference to the player's inventory grid. </summary>
        [ExportGroup("Nodes")]
        [Export] private InventoryGrid _playerGrid;

        /// <summary> A reference to the grid of the storage object the player is interacting with. </summary>
        [Export] private InventoryGrid _storageGrid;


        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<InventoryItem> ItemPool { get; private set; }


        /// <summary> The currently selected grid cell. </summary>
        private Vector2I _currentSelection = Vector2I.Zero;

        /// <summary> The grid the player is currently interacting with. </summary>
        private InventoryGrid? _currentGrid = null;

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
            if(_currentGrid != null)
            {
                InventoryItem[] activeItems = ItemPool.GetActiveObjects();
                foreach (InventoryItem item in activeItems)
                {
                    item.GlobalPosition = _currentGrid.CalculatePosition(item.CellPosition);
                }
            }
        }


        /// <summary> Clean the window up if the window is hidden. </summary>
        private void OnVisibilityChanged()
        {
            //  Return a held item back to its position.
            if (_currentHeldItem != null)
            {
                Boolean isAdded = _currentHeldItem.TryPlaceItem(_currentGrid, _currentHeldItem.CellPosition);
                if (!isAdded) { DropItem(_currentHeldItem); }
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
        public void Initialise(EntityInventory playerInventory, EntityInventory storageInventory)
        {
            _playerGrid.Initialise(playerInventory, ItemPool);
            _storageGrid.Initialise(storageInventory, ItemPool);
            _currentGrid = _playerGrid;
            _currentSelection = Vector2I.Zero;

            // Get the items in the player inventory.
            Dictionary<ItemResource, Vector2I> playerItems = playerInventory.GetItems();
            foreach (KeyValuePair<ItemResource, Vector2I> item in playerItems)
            {
                CreateItem(_playerGrid, item.Key, item.Value);
            }

            // Get the items in the storage inventory.
            Dictionary<ItemResource, Vector2I> storageItems = storageInventory.GetItems();
            foreach (KeyValuePair<ItemResource, Vector2I> item in storageItems)
            {
                CreateItem(_storageGrid, item.Key, item.Value);
            }
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="grid"> The grid to spawn the item into. </param>
        /// <param name="resource"> The data to initialise the item with. </param>
        /// <param name="position"> The cell position to create the object at. This is the object's top-left corner. </param>
        /// <returns> The initialised item. </returns>
        private InventoryItem CreateItem(InventoryGrid grid, ItemResource resource, Vector2I position)
        {
            InventoryItem item = ItemPool.GetAvailableObject();
            item.Initialise(grid, resource, position);
            return item;
        }


        /// <inheritdoc/>
        public override void _Input(InputEvent @event)
        {
            if (Visible && _currentGrid != null)
            {
                if (@event is InputEventMouseMotion mouseEvent) // Allow the mouse to move / override the current selection.
                {
                    _currentSelection = _currentGrid.CalculatePosition(mouseEvent.GlobalPosition);
                }
                else
                {
                    if (Input.IsActionJustPressed("action_ui_accept"))
                    {
                        PickupItem(_currentGrid.GetInventoryItem(_currentSelection));
                    }
                    else if (@event.IsActionReleased("action_ui_cancel"))
                    {
                        DropItem(_currentGrid.GetInventoryItem(_currentSelection));
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
                        InventoryItem? item = _currentGrid.GetInventoryItem(_currentSelection);
                        if (item != null)
                        {
                            Vector2I itemSize = item.GetResource<ItemResource>().GetSize();  // We only need to apply the size if we're going 'forwards', not 'backwards' as the position for a large object will be the top-left.
                            _currentSelection += direction * new Vector2I(direction.X > 0 ? itemSize.X : 1, direction.Y > 0 ? itemSize.Y : 1);
                        }
                        else
                        {
                            _currentSelection += direction;
                        }
                    }
                }

                // Allow to move between inventory grids.
                if (_currentGrid == _playerGrid && _currentSelection.X >= _playerGrid.GridSize.X)
                {
                    _currentGrid = _storageGrid;
                    _currentSelection = new Vector2I(0, _currentSelection.Y);
                }
                else if (_currentGrid == _storageGrid && _currentSelection.X < 0)
                {
                    _currentGrid = _playerGrid;
                    _currentSelection = new Vector2I(_playerGrid.GridSize.X - 1, _currentSelection.Y);
                }

                // Clamp the selection to the grid.
                _currentSelection = _currentSelection.Clamp(Vector2I.Zero, _currentGrid.GridSize - Vector2I.One);

                // Check if we've landed on an object, and need to move to its top left corner.
                InventoryItem? currentItem = _currentGrid.GetInventoryItem(_currentSelection);
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
                Boolean isAdded = _currentHeldItem.TryPlaceItem(_currentGrid, _currentSelection);
                if (!isAdded) { DropItem(_currentHeldItem); }
                _currentHeldItem = null;
            }
        }


        /// <summary> Drop / cancel the grab of the given item. </summary>
        /// <param name="item"> The item to manipulate, or null if an empty position was selected. </param>
        private void DropItem(InventoryItem? item)
        {
            if (item != null)
            {
                Vector3 playerPosition = ActorController.Instance.GetPlayer().GlobalPosition;
                ItemController.Instance.CreateItem(item.GetResource<ItemResource>(), playerPosition);
                item.FreeObject();
            }
        }


        /// <inheritdoc/>
        public override void _Draw()
        {
            if (Visible && _currentGrid != null)
            {
                Int32 cellSize = InventoryGrid.CELL_SIZE;

                // This relies upon the sort order of this and the grid texture being manually set.
                Vector2 start = _currentGrid.Position + (_currentSelection * cellSize);

                Vector2 size = Vector2.One * cellSize;
                ItemResource? currentItem = _currentGrid.Inventory?.GetItem(_currentSelection);
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
            if (_currentHeldItem != null && _currentGrid != null)
            {
                _currentHeldItem.GlobalPosition = _currentGrid.CalculatePosition(_currentSelection);
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
