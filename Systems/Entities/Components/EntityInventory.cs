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
        [Export] private Vector2I _inventorySize = new Vector2I(10, 10);


        /// <summary> The items being stored in the entity's inventory. </summary>
        public ItemDataComponent[,] StoredItems { get; private set; }


        /// <inheritdoc/>
        public override void _Ready()
        {
            StoredItems = new ItemDataComponent[_inventorySize.X, _inventorySize.Y];
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
                Vector2I currentPosition = new Vector2I(Mathf.Clamp(position.GetValueOrDefault().X, 0, _inventorySize.X - 1), Mathf.Clamp(position.GetValueOrDefault().Y, 0, _inventorySize.Y - 1));
                if (StoredItems[currentPosition.X, currentPosition.Y] == null)
                {
                    isAdded = SetItem(item, currentPosition);
                }
            }
            else
            {
                // Look for available positions.
                for (Int32 x = 0; x < StoredItems.GetLength(0); x++)
                {
                    for (Int32 y = 0; y < StoredItems.GetLength(1); y++)
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
                    StoredItems[currentPosition.X, currentPosition.Y] = item;
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

            // Look for available positions.
            for (Int32 x = 0; x < StoredItems.GetLength(0); x++)
            {
                for (Int32 y = 0; y < StoredItems.GetLength(1); y++)
                {
                    if (StoredItems[x, y] != null)
                    {
                        foreach (Vector2I point in item.Points)
                        {
                            if (StoredItems[x + point.X, y + point.Y] != null)
                            {
                                doesFit = false;
                                break;
                            }
                        }
                    }
                }
                if (!doesFit) { break; }
            }

            return doesFit;
        }


        /// <summary> Remove an item from the inventory. </summary>
        /// <param name="item"> The item to remove. </param>
        public void RemoveItem(ItemDataComponent item)
        {
            for (Int32 x = 0; x < StoredItems.GetLength(0); x++)
            {
                for (Int32 y = 0; y < StoredItems.GetLength(1); y++)
                {
                    if (StoredItems[x, y] == item)
                    {
                        StoredItems[x, y] = null;
                    }
                }
            }
        }
    }
}
