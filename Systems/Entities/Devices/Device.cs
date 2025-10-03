using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Interfaces;
using System;

namespace Khepri.Entities.Devices
{
    public partial class Device : StaticBody3D, IEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private AnimatedSprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <summary> The data component representing the device's data. </summary>
        public DeviceData Data { get; private set; }


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <summary> Using an item tries to pick it up. </summary>
        public void Use(IEntity activatingEntity)
        {
            if (activatingEntity is Unit unit)
            {
                throw new NotImplementedException();
            }
        }
    }
}
