using Godot;
using Khepri.Entities.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Components
{
    /// <summary> A data structure representing an entity's inventory. </summary>
    public partial class EntityInventory : Node
    {
        /// <summary> The dimensions of the inventory's grid. </summary>
        [ExportGroup("Settings")]
        [Export] public Vector2I InventorySize { get; private set; } = new Vector2I(10, 10);


        /// <summary> The items being stored in the entity's inventory. </summary>
        private ItemData[,] _storedItems;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _storedItems = new ItemData[InventorySize.X, InventorySize.Y];
        }


        /// <summary> Attempt to add an item to the inventory. </summary>
        /// <param name="item"> The item to add. </param>
        /// <param name="position"> The position within the inventory to add the item to. Null means trying to make it fit in the first available space. </param>
        /// <returns> Whether the item was added successfully. </returns>
        public Boolean TryAddItem(ItemData item, Vector2I? position = null)
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
                for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
                {
                    for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
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
        private Boolean SetItem(ItemData item, Vector2I position)
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
        private Boolean CheckItemFits(ItemData item, Vector2I position)
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
        public void RemoveItem(ItemData item)
        {
            for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
            {
                for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
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
        public ItemData? GetItem(Vector2I position) => GetItem(position.X, position.Y);


        /// <summary> Attempt to get the item at the given cell position. </summary>
        /// <param name="x"> The horizontal component. </param>
        /// <param name="y"> The vertical component. </param>
        /// <returns> The found item, or null if there was none. </returns>
        public ItemData? GetItem(Int32 x, Int32 y)
        {
            if (x >= InventorySize.X || y >= InventorySize.Y)
            {
                return null;
            }
            return _storedItems[x, y] ?? null;
        }


        /// <summary> Attempt to get all the items with a particular kind / name from the inventory. </summary>
        /// <param name="name"> The item's unique key / name. </param>
        /// <returns> An array of found item data. </returns>
        public ItemData[] GetItem(String name)
        {
            HashSet<ItemData> uniqueItems = new HashSet<ItemData>();

            for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
            {
                for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
                {
                    ItemData? currentItem = GetItem(x, y);
                    if (currentItem != null)
                    {
                        uniqueItems.Add(currentItem);
                    }
                }
            }

            return uniqueItems.ToArray();
        }


        /// <summary> Attempt to get a specific instance of an item by searching for its unique id. </summary>
        /// <param name="uid"> The item instance's unique id. </param>
        /// <returns> The found item, or null if there was none. </returns>
        public ItemData? GetItem(Guid uid)
        {
            for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
            {
                for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
                {
                    ItemData? currentItem = GetItem(x, y);
                    if (currentItem != null && currentItem.UId == uid)
                    {
                        return currentItem;
                    }
                }
            }

            return null;
        }


        /// <summary> Get the top-left position of an item in the inventory. </summary>
        /// <param name="item"> The item to search for. </param>
        /// <returns> The found grid coordinates. A null means that the item isn't in the inventory. </returns>
        public Vector2I? GetItemPosition(ItemData item)
        {
            for (Int32 y = 0; y < _storedItems.GetLength(1); y++)
            {
                for (Int32 x = 0; x < _storedItems.GetLength(0); x++)
                {
                    if (GetItem(x, y) == item)
                    {
                        return new Vector2I(x, y);
                    }
                }
            }

            return null;
        }


        /// <summary> Checks to see if the inventory contains an instance of a kind of item. </summary>
        /// <param name="name"> The item's unique key / name. </param>
        /// <returns> The number of that kind of item in the inventory. </returns>
        public Int32 HasItem(String name) => GetItem(name).Length;


        /// <summary> Checks to see if the inventory contains a specific item. </summary>
        /// <param name="uid"> The item's unique identifier. </param>
        /// <returns> Whether the specific item is in the inventory. </returns>
        public Boolean HasItem(Guid uid) => GetItem(uid) != null;
    }
}
