using System;
using Godot;

namespace Khepri.Entities.Items.Components
{
    /// <summary> A type of entity representing something that can be grabbed and placed in an inventory. </summary>
    public class ItemDataComponent  // TODO - Add unique from id, or something.
    {
        /// <summary> What kind of item it is. </summary>
        public ItemType ItemType { get; private set; } = ItemType.FOOD;

        /// <summary> The index of the sprites used to represent this item. </summary>
        public Int32 SpriteIndex { get; private set; } = 0;

        /// <summary> Relative points representing the grid cells the item occupies in an inventory. </summary>
        public Vector2I[] Points { get; private set; } = Array.Empty<Vector2I>();


        public ItemDataComponent(Vector2I[] points)
        {
            Points = points;
        }
    }


    /// <summary> A kind or category of item. </summary>
    public enum ItemType
    {
        NONE,
        FOOD,
        WEAPON,
        TOOL,
        APPEAL,
        EQUIPMENT,
        ACCESSORY
    }
}
