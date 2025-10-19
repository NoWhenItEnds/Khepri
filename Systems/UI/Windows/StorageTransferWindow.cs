using Godot;
using Khepri.Types;
using Khepri.UI.Windows.Components;
using System;

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


        /// <summary> The grid the player is currently interacting with. </summary>
        private InventoryGrid _currentGrid;


        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<InventoryItem> ItemPool { get; private set; }


        /// <inheritdoc/>
        public override void _Ready()
        {
            ItemPool = new ObjectPool<InventoryItem>(this, _inventoryItemPrefab);
            foreach (InventoryItem item in ItemPool.GetAllObjects())
            {
                item.Spawn(ItemPool);
                //item.ItemPressed += PickupItem;
            }

            //GetViewport().SizeChanged += OnSizeChanged;
            //VisibilityChanged += OnVisibilityChanged;
        }
    }
}
