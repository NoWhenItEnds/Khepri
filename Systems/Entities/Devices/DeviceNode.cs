using Godot;
using Khepri.Entities.Actors;
using Khepri.Resources;
using Khepri.Resources.Devices;
using Khepri.Types;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> An entity that can be interacted with by a unit for a unique effect. </summary>
    public partial class DeviceNode : StaticBody3D, IEntity, IControllable, IPoolable
    {
        /// <summary> The device's bounding shape. </summary>
        [ExportGroup("Nodes")]
        [Export] private CollisionShape3D _collisionShape;

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private AnimatedSprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <summary> The data resource representing the device. </summary>
        [ExportGroup("Statistics")]
        [Export] private DeviceResource _resource;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;
        }


        /// <summary> When a unit enters the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyEntered(Node3D body)
        {
            if (body is Being unit)
            {
                unit.AddUsableEntity(this);
            }
        }


        /// <summary> When a unit leaves the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is Being unit)
            {
                unit.RemoveUsableEntity(this);
            }
        }


        /// <summary> Initialise the node with new data values. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        public void Initialise(DeviceResource resource, Vector3 position)
        {
            _resource = resource;
            GlobalPosition = position;
            _sprite.SpriteFrames = _resource.WorldSprites;
            _sprite.Play();
        }


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingBeing"> A reference to the unit activating the action. </param>
        public void Examine(Being activatingBeing) => _resource.Examine(activatingBeing);


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingBeing"> A reference to the unit activating the action. </param>
        public void Use(Being activatingBeing) => _resource.Use(activatingBeing);


        /// <inheritdoc/>
        public Vector3 GetWorldPosition() => GlobalPosition;


        /// <inheritdoc/>
        public CollisionShape3D GetCollisionShape() => _collisionShape;


        /// <inheritdoc/>
        public T GetResource<T>() where T : EntityResource
        {
            if (_resource is T resource)
            {
                return resource;
            }
            else
            {
                throw new InvalidCastException($"Unable to cast the resource to {typeof(T)}.");
            }
        }


        /// <inheritdoc/>
        public void HandleInput(IInput input)
        {
            if(_resource is IControllable controllable)
            {
                controllable.HandleInput(input);
            }
        }


        /// <inheritdoc/>
        public void FreeObject() => throw new NotImplementedException();    // TODO - Implements.


        /// <inheritdoc/>
        public Godot.Collections.Dictionary<String, Variant> Serialise()
        {
            throw new NotImplementedException();
        }
    }
}
