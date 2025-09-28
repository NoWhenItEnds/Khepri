using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors.Components;
using Khepri.Entities.Actors.Components.States;
using Khepri.Entities.Interfaces;
using Khepri.Models.Input;
using Khepri.Nodes;
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

        /// <summary> A reference to the unit's sprite. </summary>
        [Export] public UnitSprite AnimatedSprite { get; private set; }

        /// <summary> A reference to the unit's needs component. </summary>
        [Export] public NeedComponent Needs { get; private set; }

        /// <summary> A reference to the unit's sensors' component. </summary>
        [Export] public SensorComponent Sensors { get; private set; }

        /// <summary> The state machine controlling the unit. </summary>
        [Export] public UnitStateMachine StateMachine { get; private set; }


        /// <summary> The animation sheets to use for the unit's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Godot.Collections.Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;


        /// <summary> Whether the unit's needs should be shown as a debug overlay. </summary>
        [ExportGroup("Debug")]
        [Export] private Boolean _showNeeds = false;


        /// <inheritdoc/>
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;

        /// <summary> The current direction the unit is facing. </summary>
        public Single Direction { get; private set; } = 0f;

        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;

        /// <summary> A list of entities that the unit is close enough to interact with. </summary>
        private HashSet<IEntity> _usableEntities = new HashSet<IEntity>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;

            // TODO - Move this to the state machine.
            // Setup the sprite animations.
            foreach (var frames in _spriteFrames)
            {
                AnimatedSprite.SetSpriteLayer(frames.Key, frames.Value);
            }
            AnimatedSprite.Play();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            UpdateDirection();

            // Show debug stuff.
            DrawDebug();
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


        private void DrawDebug()
        {
            Vector3 current = GlobalPosition + new Vector3(1, 1, -1);
            // Show debug stuff.
            if (_showNeeds)
            {
                DebugDraw3D.DrawText(current, $"HEA: {Needs.CurrentHealth:F1}", 32, Colors.Green);
                current += Vector3.Back * 0.2f;
                DebugDraw3D.DrawText(current, $"HUG: {Needs.CurrentHunger:F1}", 32, Colors.Green);
                current += Vector3.Back * 0.2f;
                DebugDraw3D.DrawText(current, $"FAT: {Needs.CurrentFatigue:F1}", 32, Colors.Green);
                current += Vector3.Back * 0.2f;
                DebugDraw3D.DrawText(current, $"ENT: {Needs.CurrentEntertainment:F1}", 32, Colors.Green);
                current += Vector3.Back * 0.2f;
                DebugDraw3D.DrawText(current, $"STA: {Needs.CurrentStamina:F1}", 32, Colors.Green);
                current += Vector3.Back * 0.2f;
            }
        }


        /// <summary> Handle the input sent to the unit by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input) => StateMachine.HandleInput(input);


        public void AddUsableEntity(IEntity entity)
        {
            _usableEntities.Add(entity);
        }

        public void RemoveUsableEntity(IEntity entity)
        {
            _usableEntities.Remove(entity);
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(UId);


        /// <inheritdoc/>
        public override Boolean Equals(Object obj)
        {
            Unit? other = obj as Unit;
            return other != null ? UId.Equals(other.UId) : false;
        }


        /// <inheritdoc/>
        public bool Equals(IEntity other) => UId.Equals(other.UId);
    }
}
