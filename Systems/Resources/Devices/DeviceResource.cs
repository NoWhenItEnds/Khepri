using Godot;
using System;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a device entity. </summary>
    [GlobalClass]
    public partial class DeviceResource : EntityResource
    {
        /// <summary> An array of descriptions for the given device. </summary>
        [ExportGroup("General")]
        [Export] public String[] Descriptions { get; private set; }

        /// <summary> The prefab used to spawn this device. </summary>
        [Export] public PackedScene Prefab { get; private set; }


        /// <summary> A reference to the sprites the device uses in the world. </summary>
        [ExportGroup("Sprites")]
        [Export] public SpriteFrames WorldSprites { get; private set; }


        /// <summary> The data component of a device entity. </summary>
        public DeviceResource() { }
    }
}
