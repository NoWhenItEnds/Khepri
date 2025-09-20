using Godot;
using Godot.Collections;
using Khepri.Entities.Interfaces;
using Khepri.Entities.Sensors;
using Khepri.Entities.UnitStates;
using Khepri.Models.Input;
using Khepri.Models.Persistent;
using Khepri.Nodes;
using System;

namespace Khepri.Entities
{
    /// <summary> An active entity controlled by something. </summary>
    public partial class Unit : CharacterBody3D, ISmartEntity
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

        /// <summary> A reference to the sensors the unit uses to see the world. </summary>
        [Export] public UnitSensors Sensors { get; private set; }

        /// <summary> The stats used by the unit to set its state. </summary>
        public readonly UnitData Data = new UnitData();


        /// <summary> The animation sheets to use for the unit's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;


        /// <inheritdoc/>
        public Guid UId { get; private set; } = Guid.NewGuid(); // TODO - This should be a part of persistent stats.

        /// <inheritdoc/>
        public Vector3 WorldPosition => GlobalPosition;


        /// <summary> The current state of the unit. </summary>
        private UnitState _currentState;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _currentState = new IdleState(this);

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
            _currentState.Update(delta);
        }


        /// <summary> Attempts to set the unit's state. </summary>
        /// <param name="state"> The state to set the unit. </param>
        public void TrySetUnitState(Type state) => _currentState = _currentState.TryTransitionTo(state);


        /// <summary> Handle the input sent to the unit by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input)
        {
            _currentState.HandleInput(input);
        }


        /// <inheritdoc/>
        public int CompareTo(IEntity other)
        {
            return UId.CompareTo(other.UId);
        }
    }
}
