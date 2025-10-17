using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors.Components;
using Khepri.Entities.Actors.States;
using Khepri.Entities.Items;
using Khepri.Resources;
using Khepri.Resources.Actors;
using Khepri.Types;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Actors
{
    /// <summary> An active entity controlled by something. </summary>
    public partial class ActorNode : CharacterBody3D, IEntity, IControllable, IPoolable
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] private CollisionShape3D _collisionShape;

        /// <summary> A reference to the actor's navigation agent. </summary>
        [Export] public NavigationAgent3D NavigationAgent { get; private set; }

        /// <summary> The position to position a camera when it's following this actor. </summary>
        [Export] public Marker3D CameraPosition { get; private set; }

        /// <summary> A component that can be polled to check if the actor is currently visible to the camera. </summary>
        [Export] public VisibleOnScreenNotifier3D VisibilityNotifier { get; private set; }

        /// <summary> A reference to the actor's sprite. </summary>
        [Export] public SpriteComponent Sprite { get; private set; }

        /// <summary> A reference to the actor's sensors' component. </summary>
        [Export] public SensorComponent Sensors { get; private set; }


        /// <summary> The grid size of the actor's inventory. </summary>
        [ExportGroup("Settings")]
        [Export] private Vector2I _inventorySize = new Vector2I(10, 10);

        /// <summary> The actor's data component. </summary>
        [Export] private ActorResource _resource;

        /// <summary> A reference to the actor's inventory component. </summary>
        public EntityInventory Inventory { get; private set; }


        /// <summary> The animation sheets to use for the actor's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Godot.Collections.Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;


        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;

        /// <summary> The current direction the actor is facing. </summary>
        public Single Direction { get; private set; } = 0f;

        public ActorStateMachine StateMachine { get; private set; }

        /// <summary> A list of entities that the actor is close enough to interact with. </summary>
        public HashSet<IEntity> UsableEntities = new HashSet<IEntity>();

        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;

        /// <summary> A reference to the actor's world controller. </summary>
        private ActorController _actorController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;
            _actorController = ActorController.Instance;
            StateMachine = new ActorStateMachine(this);
            Inventory = new EntityInventory(_inventorySize);

            // Setup the sprite animations.
            foreach (var frames in _spriteFrames)
            {
                Sprite.SetSpriteLayer(frames.Key, frames.Value);
            }
            Sprite.Play();
        }



        /// <summary> Initialise the actor with its starting values. </summary>
        /// <param name="resource"> The actor's data component. </param>
        /// <param name="position"> The world position to set the actor at. </param>
        public void Initialise(ActorResource resource, Vector3 position)
        {
            GlobalPosition = position;
            _resource = resource;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            StateMachine.Update(delta);
            //_resource.Update();
            UpdateDirection();
        }


        /// <summary> Update the direction the actor is currently facing. </summary>
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
        public void FreeObject() => ActorController.Instance.ActorPool.FreeObject(this);


        /// <inheritdoc/>
        public Vector3 GetWorldPosition() => GlobalPosition;


        /// <inheritdoc/>
        public CollisionShape3D GetCollisionShape() => _collisionShape;


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
        public void Examine(ActorNode activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Use(ActorNode activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public Godot.Collections.Dictionary<String, Variant> Serialise()
        {
            Godot.Collections.Dictionary<String, Variant> data = _resource.Serialise();
            data.Add("instance", GetInstanceId());
            data.Add("position", GlobalPosition);
            return data;
        }


        /// <inheritdoc/>
        public void Deserialise(Godot.Collections.Dictionary<String, Variant> data)
        {
            _resource.Deserialise(data);
        }
    }
}
