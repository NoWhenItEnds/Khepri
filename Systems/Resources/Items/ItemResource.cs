using Godot;
using Godot.Collections;
using Khepri.Entities.Actors;
using System.Linq;

namespace Khepri.Resources.Items
{
    /// <summary> The data component of an item entity. </summary>
    public abstract partial class ItemResource : EntityResource
    {
        /// <summary> A reference to the sprite the item uses in the world. </summary>
        [ExportGroup("Sprites")]
        [Export] public Texture2D WorldSprite { get; private set; }

        /// <summary> A reference to the sprite the item uses in the inventory. </summary>
        [Export] public Texture2D InventorySprite { get; private set; }

        /// <summary> The positions of the cells the item will take up in the inventory. </summary>
        [Export] public Array<Vector2I> InventoryCells { get; private set; }


        /// <summary> The data component of an item entity. </summary>
        public ItemResource() { }


        /// <summary> Gets the size of the item's bounding box by looking for its largest point. </summary>
        /// <returns> The size of the item's bounding box. </returns>
        public Vector2I GetSize() => new Vector2I(InventoryCells.Max(x => x.X) + 1, InventoryCells.Max(x => x.Y) + 1);  // +1 because otherwise it would be the start of the cell, instead of the end.


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public abstract void Use(ActorNode activatingEntity);
    }
}
