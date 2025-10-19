using Godot;
using Khepri.Entities.Items;
using Khepri.Resources.Items;
using Khepri.Types;
using Khepri.Types.Exceptions;
using System;
using System.Linq;

namespace Khepri.UI.Windows.Components
{
    /// <summary> A component representing a single entity's inventory. </summary>
    public partial class InventoryGrid : TextureRect
    {
        /// <summary> A reference to the entity's inventory this grid represents. </summary>
        /// <remarks> A null indicates that one isn't currently set. </remarks>
        public EntityInventory? Inventory = null;

        /// <summary> The number of cells in the inventory. </summary>
        public Vector2I GridSize = Vector2I.One;


        /// <summary> A reference to the window's item pool. </summary>
        private ObjectPool<InventoryItem>? _itemPool = null;


        /// <summary> The size of the cell in pixels. </summary>
        public const Int32 CELL_SIZE = 32;


        /// <summary> Initialise the grid by referencing an entity's inventory. </summary>
        /// <param name="inventory"> The entity's inventory. </param>
        /// <param name="itemPool"> A reference to the window's item pool. </param>
        public void Initialise(EntityInventory inventory, ObjectPool<InventoryItem> itemPool)
        {
            Inventory = inventory;
            _itemPool = itemPool;

            // Set inventory size.
            GridSize = Inventory.InventorySize;
            Single halfSize = CELL_SIZE * 0.5f;
            OffsetTop = -GridSize.X * halfSize;
            OffsetBottom = GridSize.X * halfSize;
            OffsetLeft = -GridSize.Y * halfSize;
            OffsetRight = GridSize.Y * halfSize;
        }


        /// <summary> Get a given inventory item at a given position. </summary>
        /// <param name="position"> The cell position to check. </param>
        /// <returns> The returned inventory item. Null means that there wasn't one at this position. </returns>
        public InventoryItem? GetInventoryItem(Vector2I position)
        {
            if(Inventory == null || _itemPool == null)
            {
                throw new UIException("The inventory and item pool should have been set before trying to call this method.");
            }

            ItemResource? item = Inventory.GetItem(position);
            if (item == null)
            {
                return null;
            }

            return _itemPool.GetActiveObjects().Where(x => x.GetResource<ItemResource>() == item).FirstOrDefault() ?? null;
        }


        /// <summary> Calculate the screen position of a given cell coordinate. </summary>
        /// <param name="position"> The cell coordinate. </param>
        /// <returns> The screen position of the cell's top left corner.</returns>
        public Vector2 CalculatePosition(Vector2I position)
        {
            Vector2 clampedPosition = new Vector2(Mathf.Clamp(position.X, 0, GridSize.X - 1), Mathf.Clamp(position.Y, 0, GridSize.Y - 1));
            return GlobalPosition + clampedPosition * CELL_SIZE;
        }


        /// <summary> Calculate the grid coordinate a given screen position. </summary>
        /// <param name="position"> The screen coordinate. </param>
        /// <returns> The cell coordinate of the cell's top left corner.</returns>
        public Vector2I CalculatePosition(Vector2 position)
        {
            Vector2 relativePosition = (position - GlobalPosition) / CELL_SIZE;
            return new Vector2I((Int32)Mathf.Clamp(relativePosition.X, 0, GridSize.X - 1), (Int32)Mathf.Clamp(relativePosition.Y, 0, GridSize.Y - 1));
        }
    }
}
