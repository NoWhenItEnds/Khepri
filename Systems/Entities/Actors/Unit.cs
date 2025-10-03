using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors.Components;
using Khepri.Entities.Actors.Components.States;
using Khepri.Entities.Components;
using Khepri.Entities.Interfaces;
using Khepri.Models.Input;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Actors
{
    /// <summary> An active entity controlled by something. </summary>
    public partial class Unit : CharacterBody3D, IEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> A reference to the unit's navigation agent. </summary>
        [Export] public NavigationAgent3D NavigationAgent { get; private set; }

        /// <summary> The position to position a camera when it's following this unit. </summary>
        [Export] public Marker3D CameraPosition { get; private set; }

        /// <summary> A component that can be polled to check if the unit is currently visible to the camera. </summary>
        [Export] public VisibleOnScreenNotifier3D VisibilityNotifier { get; private set; }

        /// <summary> A reference to the unit's sprite. </summary>
        [Export] public SpriteComponent Sprite { get; private set; }

        /// <summary> A reference to the unit's needs component. </summary>
        [Export] public NeedComponent Needs { get; private set; }

        /// <summary> A reference to the unit's sensors' component. </summary>
        [Export] public SensorComponent Sensors { get; private set; }

        /// <summary> The state machine controlling the unit. </summary>
        [Export] public UnitStateMachine StateMachine { get; private set; }

        /// <summary> A reference to the unit's inventory component. </summary>
        [Export] public EntityInventory Inventory { get; private set; }


        /// <summary> The animation sheets to use for the unit's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Godot.Collections.Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;

        /// <summary> The current direction the unit is facing. </summary>
        public Single Direction { get; private set; } = 0f;

        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;

        /// <summary> A list of entities that the unit is close enough to interact with. </summary>
        public HashSet<IEntity> UsableEntities = new HashSet<IEntity>();


        /// <inheritdoc/>
        public void Use(IEntity activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;

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
            UpdateDirection();
        }


        /// <summary> Update the direction the unit is currently facing. </summary>
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


        /// <summary> Handle the input sent to the unit by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input) => StateMachine.HandleInput(input);


        public void AddUsableEntity(IEntity entity)
        {
            UsableEntities.Add(entity);
        }

        public void RemoveUsableEntity(IEntity entity)
        {
            UsableEntities.Remove(entity);
        }
    }
}
