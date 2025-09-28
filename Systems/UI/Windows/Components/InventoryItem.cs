using Godot;
using Khepri.Entities.Components;
using Khepri.Entities.Items.Components;
using System;

namespace Khepri.UI.Windows.Components
{
    /// <summary> An inventory item in an inventory grid. </summary>
    public partial class InventoryItem : TextureButton
    {
        /// <summary> The position of the item's top-left position. </summary>
        public Vector2I CellPosition { get; private set; }

        /// <summary> The item this button represents. </summary>
        public ItemDataComponent Data { get; private set; }


        /// <summary> A reference to the game world's viewport. </summary>
        private Viewport _viewport;

        /// <summary> A reference to the inventory this item is a part of. </summary>
        private EntityInventory _inventory;

        /// <summary> A reference to the window this item is parented to. </summary>
        private InventoryWindow _window;


        /// <summary> Whether the item is currently following the mouse. It will when it is clicked. </summary>
        private Boolean _isFollowingMouse = false;


        /// <inheritdoc/>
        public override void _Ready()
        {
            ButtonDown += OnButtonDown;
            ButtonUp += OnButtonUp;

            _viewport = GetViewport();
        }


        /// <summary> Pick the item up. </summary>
        private void OnButtonDown()
        {
            _isFollowingMouse = true;
            _inventory.RemoveItem(Data);
        }


        /// <summary> Drop the item in the desired spot. </summary>
        private void OnButtonUp()
        {
            _isFollowingMouse = false;

            Vector2 mousePosition = _viewport.GetMousePosition();
            Vector2I cellPosition = _window.CalculatePosition(mousePosition);
            Boolean isAdded = _inventory.TryAddItem(Data, cellPosition);

            // If the item was added, update its position.
            if (isAdded)
            {
                CellPosition = cellPosition;
                GlobalPosition = _window.CalculatePosition(cellPosition);
            }
            else    // Snap it back to its previous position.
            {
                Boolean isReturned = _inventory.TryAddItem(Data, CellPosition);
                if (!isReturned)    // If it couldn't be returned, for whatever reason, drop it.
                {
                    // TODO - Drop it.
                }
            }
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



        /// <summary> Initialise the item by giving data. </summary>
        /// <param name="data"> The raw data to build the item. </param>
        /// <param name="cellPosition"> The position of the item's top-left position. </param>
        /// <param name="inventory"> A reference to the inventory this item is a part of. </param>
        /// <param name="window"> A reference to the window this item is parented to. </param>
        public void Initialise(ItemDataComponent data, Vector2I cellPosition, EntityInventory inventory, InventoryWindow window)
        {
            Data = data;
            CellPosition = cellPosition;
            _inventory = inventory;
            _window = window;
        }


        /// <summary> Set the item's grid cell position. </summary>
        /// <param name="cellPosition"> The position of the item's top-left position. </param>
        public void SetPosition(Vector2I cellPosition)
        {
            CellPosition = cellPosition;
        }
    }
}
