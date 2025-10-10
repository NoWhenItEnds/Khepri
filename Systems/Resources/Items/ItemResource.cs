using Godot;
using Godot.Collections;
using Khepri.Entities.Actors;
using System;
using System.Linq;

namespace Khepri.Resources.Items
{
    /// <summary> The data component of an item entity. </summary>
    public abstract partial class ItemResource : EntityResource
    {
        /// <summary> An array of descriptions for the given item. </summary>
        [ExportGroup("General")]
        [Export] public String[] Descriptions { get; private set; }

        /// <summary> The category of item. </summary>
        public abstract ItemType ItemType { get; }


        /// <summary> A reference to the sprites the item uses to lookup its world sprite. </summary>
        [ExportGroup("Sprites")]
        [Export] public SpriteFrames WorldSprites { get; private set; }

        /// <summary> A reference to the sprites the item uses in the inventory. </summary>
        [Export] public Texture2D InventorySprite { get; private set; }

        /// <summary> The item's index on the sprite sheet. </summary>
        [Export] public Int32 SpriteIndex { get; private set; }

        /// <summary> The positions of the cells the item will take up in the inventory. </summary>
        [Export] public Array<Vector2I> InventoryCells { get; private set; }


        /// <summary> The data component of an item entity. </summary>
        public ItemResource() { }


        /// <summary> Gets the size of the item's bounding box by looking for its largest point. </summary>
        /// <returns> The size of the item's bounding box. </returns>
        public Vector2I GetSize() => new Vector2I(InventoryCells.Max(x => x.X) + 1, InventoryCells.Max(x => x.Y) + 1);  // +1 because otherwise it would be the start of the cell, instead of the end.


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public abstract void Examine(Unit activatingEntity);


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public abstract void Use(Unit activatingEntity);
    }


    /// <summary> The category of item. </summary>
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
