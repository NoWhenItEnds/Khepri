using System;
using Godot;
using Khepri.Models.Input;
using Khepri.Types;

namespace Khepri.Entities.Actors.Components.States
{
    /// <summary> The unit is standing idle, waiting for an action. </summary>
    public class IdlingState : UnitState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "Idle_";


        /// <summary> The unit is standing idle, waiting for an action. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public IdlingState(Unit unit) : base(unit) { }


        /// <inheritdoc/>
        public override void HandleInput(IInput input) { }


        /// <inheritdoc/>
        public override void Update(Double delta)
        {
            // TODO - Have a timer here to do a fidget animation.
            _unit.AnimatedSprite.TransitionAnimation(this, _unit.Direction.ToDirection());

            _unit.Velocity = new Vector3(0f, 0f, 0f);

            // Apply gravity if we're not on the ground.
            if (!_unit.IsOnFloor())
            {
                _unit.Velocity -= new Vector3(0f, 9.81f, 0f) * 0.5f * (Single)delta;
            }

            _unit.MoveAndSlide();
        }
    }
}
