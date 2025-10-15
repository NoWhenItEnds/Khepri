using System;
using Godot;
using Khepri.Resources.Actors;
using Khepri.Types;

namespace Khepri.Entities.Actors.Components.States
{
    /// <summary> The unit is walking across the ground. </summary>
    public class WalkingState : UnitState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "Walk_";


        /// <summary> The unit is walking across the ground. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public WalkingState(Being unit) : base(unit) { }


        /// <inheritdoc/>
        public override void HandleInput(IInput input)
        {
            if (input is MoveInput move)
            {
                _unit.Velocity = move.Direction * _unit.GetResource<BeingResource>().Needs.BaseSpeed;
                _unit.Sprite.TransitionAnimation(this, move.Direction.ToDirection());
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
