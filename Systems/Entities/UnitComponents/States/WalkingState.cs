using System;
using Godot;
using Khepri.Models.Input;
using Khepri.Types;

namespace Khepri.Entities.UnitComponents.States
{
    /// <summary> The unit is walking across the ground. </summary>
    public class WalkingState : UnitState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "walking_";


        /// <summary> The unit is walking across the ground. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public WalkingState(Unit unit) : base(unit) { }


        /// <inheritdoc/>
        public override void HandleInput(IInput input)
        {
            if (input is MoveInput move)
            {
                _unit.Velocity = move.Direction * _unit.Data.BaseSpeed;
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
