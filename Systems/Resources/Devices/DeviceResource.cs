using Godot;
using Khepri.Entities.Actors;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a device entity. </summary>
    public abstract partial class DeviceResource : EntityResource
    {
        /// <summary> A reference to the sprites the device uses in the world. </summary>
        [ExportGroup("Sprites")]
        [Export] public SpriteFrames WorldSprites { get; private set; }


        /// <summary> The data component of a device entity. </summary>
        public DeviceResource() { }


        /// <summary> Use the device. </summary>
        /// <param name="activatingBeing"> The being activating the action. </param>
        public abstract void Use(Being activatingBeing);
    }
}
