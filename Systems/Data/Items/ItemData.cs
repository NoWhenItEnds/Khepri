using Godot;
using Godot.Collections;
using Khepri.Entities.Actors;
using Khepri.Types.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Khepri.Data.Items
{
    /// <summary> The data component of an item entity. </summary>
    public abstract class ItemData : EntityData
    {
        /// <summary> The relative, Godot, filepath to the entity's world sprites. </summary>
        [JsonPropertyName("world_sprite_filepath"), Required]
        public required String WorldSpriteFilepath { get; init; }

        /// <summary> The relative, Godot, filepath to the entity's inventory sprite. </summary>
        [JsonPropertyName("inventory_sprite_filepath"), Required]
        public required String InventorySpriteFilepath { get; init; }

        /// <summary> The positions of the cells the item will take up in the inventory. </summary>
        [JsonPropertyName("inventory_cells"), Required]
        public required Array<Vector2I> InventoryCells { get; init; }


        /// <summary> Attempt to load the entity's world sprites from the filesystem. </summary>
        /// <returns> The loaded sprite frames. </returns>
        public SpriteFrames GetWorldSprites() => IOExtensions.GetResource<SpriteFrames>(WorldSpriteFilepath);


        /// <summary> Attempt to load the entity's inventory sprite from the filesystem. </summary>
        /// <returns> The loaded sprite frames. </returns>
        public Texture2D GetInventorySprite() => IOExtensions.GetResource<Texture2D>(InventorySpriteFilepath);


        /// <summary> Gets the size of the item's bounding box by looking for its largest point. </summary>
        /// <returns> The size of the item's bounding box. </returns>
        public Vector2I GetSize() => new Vector2I(InventoryCells.Max(x => x.X) + 1, InventoryCells.Max(x => x.Y) + 1);  // +1 because otherwise it would be the start of the cell, instead of the end.


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public abstract void Use(ActorNode activatingEntity);
    }
}
