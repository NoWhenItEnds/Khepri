using Godot;
using Khepri.Entities.Actors;
using Khepri.Resources.Devices;
using Khepri.Types;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> An entity that can be interacted with by a unit for a unique effect. </summary>
    public abstract partial class DeviceNode : StaticBody3D, IEntity, IPoolable<DeviceResource>
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private AnimatedSprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <inheritdoc/>
        [ExportGroup("Statistics")]
        [Export] public DeviceResource Resource { get; set; }


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;

            // TODO - Move to device controller.
            _sprite.SpriteFrames = Resource.WorldSprites;
            _sprite.Play();
        }


        /// <summary> When a unit enters the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyEntered(Node3D body)
        {
            if (body is Unit unit)
            {
                unit.AddUsableEntity(this);
            }
        }


        /// <summary> When a unit leaves the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is Unit unit)
            {
                unit.RemoveUsableEntity(this);
            }
        }


        /// <inheritdoc/>
        public abstract void Examine(Unit activatingEntity);


        /// <inheritdoc/>
        public abstract void Use(Unit activatingEntity);


        /// <inheritdoc/>
        public void FreeObject() => throw new NotImplementedException();
    }
}
