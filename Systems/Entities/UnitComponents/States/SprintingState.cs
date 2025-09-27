using System;
using Godot;
using Khepri.Models.Input;
using Khepri.Types;

namespace Khepri.Entities.UnitComponents.States
{
    /// <summary> The unit is moving quickly along the ground. </summary>
    public class SprintingState : UnitState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "Walk_";  // TODO - Make sprinting animations.


        /// <summary> The unit is moving quickly along the ground. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public SprintingState(Unit unit) : base(unit) { }


        /// <inheritdoc/>
        public override void HandleInput(IInput input)
        {
            if (input is MoveInput move)
            {
                _unit.Velocity = move.Direction * _unit.Data.BaseSpeed * _unit.Data.SprintModifier;
                _unit.AnimatedSprite.TransitionAnimation(this, move.Direction.ToDirection());
            }
        }


        /// <inheritdoc/>
        public override void Update(Double delta)
        {
            // Apply gravity if we're not on the ground.
            if (!_unit.IsOnFloor())
            {
                _unit.Velocity -= new Vector3(0f, 9.81f, 0f) * 0.5f * (Single)delta;
            }

            _unit.MoveAndSlide();
        }
    }
}
