using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Godot;
using Khepri.Types.Extensions;

namespace Khepri.Data.Actors
{
    /// <summary> The data component for actor entities within the game world. </summary>
    public abstract class ActorData : EntityData
    {
        /// <summary> The relative, Godot, filepath to the entity's sprites. </summary>
        [JsonPropertyName("sprite_filepath"), Required]
        public required String SpriteFilepath { get; init; }


        /// <summary> Attempt to load the entity's world sprites from the filesystem. </summary>
        /// <returns> The loaded sprite frames. </returns>
        public SpriteFrames GetSprites() => IOExtensions.GetResource<SpriteFrames>(SpriteFilepath);
    }
}
