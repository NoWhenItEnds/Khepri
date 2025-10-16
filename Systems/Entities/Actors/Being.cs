using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors.Components;
using Khepri.Entities.Actors.States;
using Khepri.Entities.Items;
using Khepri.Resources;
using Khepri.Resources.Actors;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Actors
{
    /// <summary> An active entity controlled by something. </summary>
    public partial class Being : CharacterBody3D, IEntity, IControllable
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] private CollisionShape3D _collisionShape;

        /// <summary> A reference to the being's navigation agent. </summary>
        [Export] public NavigationAgent3D NavigationAgent { get; private set; }

        /// <summary> The position to position a camera when it's following this being. </summary>
        [Export] public Marker3D CameraPosition { get; private set; }

        /// <summary> A component that can be polled to check if the being is currently visible to the camera. </summary>
        [Export] public VisibleOnScreenNotifier3D VisibilityNotifier { get; private set; }

        /// <summary> A reference to the being's sprite. </summary>
        [Export] public SpriteComponent Sprite { get; private set; }

        /// <summary> A reference to the being's sensors' component. </summary>
        [Export] public SensorComponent Sensors { get; private set; }


        /// <summary> The grid size of the being's inventory. </summary>
        [ExportGroup("Settings")]
        [Export] private Vector2I _inventorySize = new Vector2I(10, 10);

        /// <summary> A reference to the being's inventory component. </summary>
        public EntityInventory Inventory { get; private set; }


        /// <summary> The animation sheets to use for the being's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Godot.Collections.Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;


        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;

        /// <summary> The current direction the being is facing. </summary>
        public Single Direction { get; private set; } = 0f;

        public ActorStateMachine StateMachine { get; private set; }

        /// <summary> A list of entities that the being is close enough to interact with. </summary>
        public HashSet<IEntity> UsableEntities = new HashSet<IEntity>();

        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;


        /// <summary> The being's data component. </summary>
        private BeingResource _resource;


        /// <summary> Initialise the being with its starting values. </summary>
        /// <param name="resource"> The being's data component. </param>
        /// <param name="position"> The world position to set the being at. </param>
        public void Initialise(BeingResource resource, Vector3 position)
        {
            _worldController = WorldController.Instance;
            StateMachine = new ActorStateMachine(this);
            Inventory = new EntityInventory(_inventorySize);

            GlobalPosition = position;
            _resource = resource;

            // Setup the sprite animations.
            foreach (var frames in _spriteFrames)
            {
                Sprite.SetSpriteLayer(frames.Key, frames.Value);
            }
            Sprite.Play();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            StateMachine.Update(delta);
            _resource.Needs.Update();
            UpdateDirection();
        }


        /// <summary> Update the direction the being is currently facing. </summary>
        public void UpdateDirection()
        {
            if (Velocity != Vector3.Zero)
            {
                Single directionRad = new Vector2(Velocity.X, Velocity.Z).Angle();
                if (directionRad < 0f)
                {
                    directionRad += Mathf.Tau;
                }
                Direction = Mathf.RadToDeg(directionRad);
            }
        }


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
        public void HandleInput(IInput input) => StateMachine.HandleInput(input);


        public void AddUsableEntity(IEntity entity)
        {
            UsableEntities.Add(entity);
        }

        public void RemoveUsableEntity(IEntity entity)
        {
            UsableEntities.Remove(entity);
        }


        /// <inheritdoc/>
        public void Examine(Being activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Use(Being activatingEntity)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Serialise()
        {
            throw new NotImplementedException();
        }
    }
}
