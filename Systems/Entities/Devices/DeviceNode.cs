using Godot;
using Khepri.Data;
using Khepri.Data.Devices;
using Khepri.Entities.Actors;
using Khepri.Types;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> An entity that can be interacted with by a unit for a unique effect. </summary>
    public partial class DeviceNode : StaticBody3D, IEntity, IPoolable
    {
        /// <inheritdoc/>
        public Guid DataUId => _data.UId;

        /// <inheritdoc/>
        public String DataKind => _data.Kind;

        /// <summary> The device's bounding shape. </summary>
        [ExportGroup("Nodes")]
        [Export] private CollisionShape3D _collisionShape;

        /// <summary> The sprite th use to represent the item. </summary>
        [Export] private AnimatedSprite3D _sprite;

        /// <summary> The radius to allow for interaction with the item. </summary>
        [Export] private Area3D _interactionArea;


        /// <summary> The device's data component. </summary>
        private DeviceData _data;


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
            if (body is ActorNode unit)
            {
                unit.AddUsableEntity(this);
            }
        }


        /// <summary> When a unit leaves the usable range. </summary>
        /// <param name="body"> A reference to the unit. </param>
        private void OnBodyExited(Node3D body)
        {
            if (body is ActorNode unit)
            {
                unit.RemoveUsableEntity(this);
            }
        }


        /// <summary> Initialise the node with new data values. </summary>
        /// <param name="data"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        public void Initialise(DeviceData data, Vector3 position)
        {
            _data = data;
            GlobalPosition = position;
            _sprite.SpriteFrames = _data.GetSprites();
            _sprite.Play();
        }


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingBeing"> A reference to the unit activating the action. </param>
        public void Examine(ActorNode activatingBeing) => _data.Examine(activatingBeing);


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingBeing"> A reference to the unit activating the action. </param>
        public void Use(ActorNode activatingBeing) => _data.Use(activatingBeing);


        /// <inheritdoc/>
        public Vector3 GetWorldPosition() => GlobalPosition;


        /// <inheritdoc/>
        public CollisionShape3D GetCollisionShape() => _collisionShape;


        /// <inheritdoc/>
        public T GetData<T>() where T : EntityData
        {
            if (_data is T data)
            {
                return data;
            }
            else
            {
                throw new InvalidCastException($"Unable to cast the resource to {typeof(T)}.");
            }
        }


        /// <inheritdoc/>
        public void FreeObject() => throw new NotImplementedException();    // TODO - Implements.
    }
}
