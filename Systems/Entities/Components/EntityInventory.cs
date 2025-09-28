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
        [Export] private Vector2I _inventorySize = new Vector2I(5, 5);


        /// <summary> The items being stored in the entity's inventory. </summary>
        public InventoryItem[,] StoredItems { get; private set; }


        /// <inheritdoc/>
        public override void _Ready()
        {
            StoredItems = new InventoryItem[_inventorySize.X, _inventorySize.Y];
        }


        /// <summary> Attempt to add an item to the inventory. </summary>
        /// <param name="item"> The item to add. </param>
        /// <param name="position"> The position within the inventory to add the item to. Null means trying to make it fit in the first available space. </param>
        /// <returns> Whether the item was added successfully. </returns>
        public Boolean AddItem(ItemDataComponent item, Vector2I? position = null)
        {
            Boolean isAdded = false;

            if (position != null)
            {
                Vector2I currentPosition = new Vector2I(position.GetValueOrDefault().X, position.GetValueOrDefault().Y);
                if (StoredItems[currentPosition.X, currentPosition.Y] == null)
                {
                    Boolean doesFit = CheckItemFits(item, currentPosition);
                    if (doesFit)
                    {
                        StoredItems[currentPosition.X, currentPosition.Y] = new InventoryItem(item, currentPosition);
                    }
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
                        Boolean doesFit = CheckItemFits(item, currentPosition);
                        if (doesFit)
                        {
                            StoredItems[currentPosition.X, currentPosition.Y] = new InventoryItem(item, currentPosition);
                            break;
                        }
                    }
                }
            }

            return isAdded;
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
                    InventoryItem currentPosition = StoredItems[x, y];
                    if (currentPosition != null)
                    {
                        foreach (Vector2I point in item.Points)
                        {
                            InventoryItem currentCell = StoredItems[x + point.X, y + point.Y];
                            if (currentCell != null)
                            {
                                doesFit = false;
                                break;
                            }
                        }
                    }
                }
            }

            return doesFit;
        }
    }


    /// <summary> A data object to represent an item in an inventory. </summary>
    public record InventoryItem
    {
        /// <summary> The item object this represents. </summary>
        public ItemDataComponent Item { get; private set; }

        /// <summary> The position of the item's top-left grid location. </summary>
        public Vector2I Position { get; private set; }


        public InventoryItem(ItemDataComponent item, Vector2I position)
        {
            Item = item;
            Position = position;
        }
    }
}
