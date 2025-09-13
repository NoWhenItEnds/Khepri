using Godot;
using Godot.Collections;
using Khepri.Models.Input;
using Khepri.Models.UnitStates;
using Khepri.Nodes;
using Khepri.Types;
using System;

namespace Khepri.Entities
{
    /// <summary> An active entity controlled by something. </summary>
    public partial class Unit : CharacterBody3D, ISmartEntity
    {
        /// <inheritdoc/>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape3D CollisionShape { get; private set; }

        /// <summary> The position to position a camera when it's following this unit. </summary>
        [Export] public Marker3D CameraPosition { get; private set; }

        /// <summary> A reference to the unit's sprite. </summary>
        [Export] public UnitSprite AnimatedSprite { get; private set; }


        /// <summary> The speed modifier for walking. </summary>
        [ExportGroup("Stats")]
        [Export] private Single _walkSpeed = 3f;

        /// <summary> The speed modifier for sprinting. </summary>
        [Export] private Single _sprintingSpeed = 6f;


        /// <summary> The animation sheets to use for the unit's animations. </summary>
        [ExportGroup("Resources")]
        [Export] private Dictionary<UnitSpriteLayer, SpriteFrames> _spriteFrames;

        /// <inheritdoc/>
        public Guid UId { get; private set; } = Guid.NewGuid(); // TODO - This should be set by a factory rather than the object itself.


        /// <summary> The current state of the unit. </summary>
        private IUnitState _currentState = new IdleState();


        /// <inheritdoc/>
        public override void _Ready()
        {
            // Setup the sprite animations.
            foreach (var frames in _spriteFrames)
            {
                AnimatedSprite.SetSpriteLayer(frames.Key, frames.Value);
            }
            AnimatedSprite.Play();
        }


        public void HandleInput(IInput input)
        {
            switch (input)
            {
                case MoveInput moveInput:
                    HandleMovement(moveInput);
                    break;
            }
        }

        private void HandleMovement(MoveInput input)
        {
            Single speedModifier = input.MovementType == MoveType.WALKING ? _walkSpeed : _sprintingSpeed;
            Velocity = input.Direction * speedModifier;
            MoveAndSlide();

            if (input.Direction != Vector3.Zero)
            {
                _currentState = new WalkingState();
                Direction direction = input.Direction.ToDirection();
                AnimatedSprite.TransitionAnimation(_currentState, direction);
            }
            else
            {
                _currentState = new IdleState();
                AnimatedSprite.TransitionAnimation(_currentState);
            }
        }
    }
}
