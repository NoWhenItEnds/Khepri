using Godot;
using Khepri.Entities.Items.Components;
using System;

namespace Khepri.Entities.Components
{
    /// <summary> A data structure representing an entity's inventory. </summary>
    public partial class EntityInventory : Node
    {
        /// <summary> The dimensions of the inventory's grid. </summary>
        [ExportGroup("Settings")]
        [Export] public Vector2I InventorySize { get; private set; } = new Vector2I(10, 10);


        /// <summary> The items being stored in the entity's inventory. </summary>
        private ItemDataComponent[,] _storedItems;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _storedItems = new ItemDataComponent[InventorySize.X, InventorySize.Y];
        }


        /// <summary> Attempt to add an item to the inventory. </summary>
        /// <param name="item"> The item to add. </param>
        /// <param name="position"> The position within the inventory to add the item to. Null means trying to make it fit in the first available space. </param>
        /// <returns> Whether the item was added successfully. </returns>
        public Boolean TryAddItem(ItemDataComponent item, Vector2I? position = null)
        {
            Boolean isAdded = false;

            if (position != null)
            {
                Vector2I currentPosition = new Vector2I(Mathf.Clamp(position.GetValueOrDefault().X, 0, InventorySize.X - 1), Mathf.Clamp(position.GetValueOrDefault().Y, 0, InventorySize.Y - 1));
                if (_storedItems[currentPosition.X, currentPosition.Y] == null)
                {
                    isAdded = SetItem(item, currentPosition);
                }
            }
            else
            {
                // Look for available positions.
                for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
                {
                    for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
                    {
                        Vector2I currentPosition = new Vector2I(x, y);
                        isAdded = SetItem(item, currentPosition);
                        if (isAdded) { break; }
                    }
                    if (isAdded) { break; }
                }
            }

            return isAdded;
        }


        /// <summary> Adds the item to the inventory by setting the correct cells. </summary>
        /// <param name="item"> The item to add. </param>
        /// <param name="position"> The top-left most position within the inventory to add the item to. </param>
        /// <returns> Whether the item was added successfully. </returns>
        private Boolean SetItem(ItemDataComponent item, Vector2I position)
        {
            Boolean doesFit = CheckItemFits(item, position);
            if (doesFit)
            {
                foreach (Vector2I point in item.Points)
                {
                    Vector2I currentPosition = position + point;
                    _storedItems[currentPosition.X, currentPosition.Y] = item;
                }
            }
            return doesFit;
        }


        /// <summary> Check to see if the given item will fit at the given position. </summary>
        /// <param name="item"> The item to check. </param>
        /// <param name="position"> The position of the top-left corner to check from. </param>
        /// <returns> Whether the item will fit at the given position. </returns>
        private Boolean CheckItemFits(ItemDataComponent item, Vector2I position)
        {
            Boolean doesFit = true;

            foreach (Vector2I point in item.Points)
            {
                Int32 x = position.X + point.X;
                Int32 y = position.Y + point.Y;
                if (x >= InventorySize.X || y >= InventorySize.Y || _storedItems[x, y] != null)
                {
                    doesFit = false;
                    break;
                }
            }

            return doesFit;
        }


        /// <summary> Remove an item from the inventory. </summary>
        /// <param name="item"> The item to remove. </param>
        public void RemoveItem(ItemDataComponent item)
        {
            for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
            {
                for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
                {
                    if (GetItem(x, y) == item)
                    {
                        _storedItems[x, y] = null;
                    }
                }
            }
        }


        /// <summary> Attempt to get the item at the given cell position. </summary>
        /// <param name="position"> The positional vector component. </param>
        /// <returns> The found item, or null if there was none. </returns>
        public ItemDataComponent? GetItem(Vector2I position) => GetItem(position.X, position.Y);


        /// <summary> Attempt to get the item at the given cell position. </summary>
        /// <param name="x"> The horizontal component. </param>
        /// <param name="y"> The vertical component. </param>
        /// <returns> The found item, or null if there was none. </returns>
        public ItemDataComponent? GetItem(Int32 x, Int32 y)
        {
            if (x >= InventorySize.X || y >= InventorySize.Y)
            {
                return null;
            }
            return _storedItems[x, y] ?? null;
        }


        /// <summary> Get the top-left position of an item in the inventory. </summary>
        /// <param name="item"> The item to search for. </param>
        /// <returns> The found grid coordinates. A null means that the item isn't in the inventory. </returns>
        public Vector2I? GetItemPosition(ItemDataComponent item)
        {
            for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
            {
                for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
                {
                    if (GetItem(x, y) == item)
                    {
                        return new Vector2I(x, y);
                    }
                }
            }

            return null;
        }
    }
}
