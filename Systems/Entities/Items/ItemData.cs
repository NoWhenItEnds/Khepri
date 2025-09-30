using System;
using System.Linq;
using Godot;

namespace Khepri.Entities.Items
{
    /// <summary> A type of entity representing something that can be grabbed and placed in an inventory. </summary>
    public class ItemData : IEquatable<ItemData>
    {
        public Guid UId { get; init; }

        /// <summary> The unique identifying name or key of the item. </summary>
        public String Name { get; init; }

        /// <summary> What kind of item it is. </summary>
        public ItemType ItemType { get; init; }

        /// <summary> The index of the sprites used to represent this item. </summary>
        public Int32 SpriteIndex { get; init; }

        /// <summary> Relative points representing the grid cells the item occupies in an inventory. </summary>
        public Vector2I[] Points { get; init; }


        /// <summary> Gets the size of the item's bounding box by looking for its largest point. </summary>
        /// <returns> The size of the item's bounding box. </returns>
        public Vector2I GetSize() => new Vector2I(Points.Max(x => x.X) + 1, Points.Max(x => x.Y) + 1);  // +1 because otherwise it would be the start of the cell, instead of the end.


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            ItemData? other = obj as ItemData;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(ItemData other) => UId.Equals(other.UId);
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
