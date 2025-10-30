using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Godot;
using Khepri.Entities.Actors;
using Khepri.Types.Extensions;

namespace Khepri.Data.Devices
{
    /// <summary> The data component of a device entity. </summary>
    public abstract class DeviceData : EntityData
    {
        /// <summary> The relative, Godot, filepath to the entity's sprites. </summary>
        [JsonPropertyName("sprite_filepath"), Required]
        public required String SpriteFilepath { get; init; }


        /// <summary> Use the device. </summary>
        /// <param name="activatingBeing"> The being activating the action. </param>
        public abstract void Use(ActorNode activatingBeing);


        /// <summary> Attempt to load the entity's world sprites from the filesystem. </summary>
        /// <returns> The loaded sprite frames. </returns>
        public SpriteFrames GetSprites() => IOExtensions.GetResource<SpriteFrames>(SpriteFilepath);
    }
}
