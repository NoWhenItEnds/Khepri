using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a device entity. </summary>
    public abstract partial class DeviceResource : EntityResource
    {
        /// <summary> An array of descriptions for the given device. </summary>
        [ExportGroup("General")]
        [Export] public String[] Descriptions { get; private set; }


        /// <summary> A reference to the sprites the device uses in the world. </summary>
        [ExportGroup("Sprites")]
        [Export] public SpriteFrames WorldSprites { get; private set; }


        /// <summary> An instance of random to use. </summary>
        private readonly Random RANDOM = new Random();


        /// <summary> The data component of a device entity. </summary>
        public DeviceResource() { }


        /// <summary> Examine the device. </summary>
        /// <param name="activatingBeing"> The being activating the action. </param>
        public void Examine(Being activatingBeing)
        {
            if(Descriptions.Length > 0)
            {
                Int32 index = RANDOM.Next(0, Descriptions.Length);
                String description = Descriptions[index];
                UIController.Instance.SpawnSpeechBubble(description, activatingBeing);
            }
        }


        /// <summary> Use the device. </summary>
        /// <param name="activatingBeing"> The being activating the action. </param>
        public abstract void Use(Being activatingBeing);
    }
}
