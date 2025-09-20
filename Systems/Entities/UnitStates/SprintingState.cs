using System;
using Godot;
using Khepri.Models.Input;
using Khepri.Types;

namespace Khepri.Entities.UnitStates
{
    /// <summary> The unit is moving quickly along the ground. </summary>
    public class SprintingState : UnitState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "sprinting_";

        /// <inheritdoc/>
        protected override Type[] _connectingStates { get; } =
        [
            typeof(IdleState),
            typeof(WalkingState)
        ];


        /// <summary> The unit is moving quickly along the ground. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public SprintingState(Unit unit) : base(unit) { }


        /// <inheritdoc/>
        public override void Update(Double delta)
        {
        }


        /// <inheritdoc/>
        public override void HandleInput(IInput input)
        {
            if (input is MoveInput move)
            {
                switch (move.MovementType)
                {
                    case MoveType.IDLE:
                        _unit.TrySetUnitState(typeof(IdleState));
                        break;
                    case MoveType.WALKING:
                        _unit.TrySetUnitState(typeof(WalkingState));
                        break;
                    default:
                        _unit.Velocity = move.Direction * _unit.Stats.BaseSpeed * _unit.Stats.SprintModifier;
                        if (!_unit.IsOnFloor()) { _unit.Velocity -= new Vector3(0f, 9.81f, 0f) * 0.5f; }    // Apply gravity if we're not on the ground.
                        _unit.MoveAndSlide();

                        _unit.AnimatedSprite.TransitionAnimation(this, move.Direction.ToDirection());
                        break;
                }
            }
        }


        /// <inheritdoc/>
        protected override void Initialise() { }


        /// <inheritdoc/>
        protected override void Cleanup() { }
    }
}
