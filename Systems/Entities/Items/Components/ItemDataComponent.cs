using System;
using Godot;

namespace Khepri.Entities.Items.Components
{
    /// <summary> A type of entity representing something that can be grabbed and placed in an inventory. </summary>
    public class ItemDataComponent : IEquatable<ItemDataComponent>
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


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            ItemDataComponent? other = obj as ItemDataComponent;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(ItemDataComponent other) => UId.Equals(other.UId);
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
