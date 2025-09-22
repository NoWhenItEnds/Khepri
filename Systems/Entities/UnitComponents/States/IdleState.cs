using System;
using Godot;
using Khepri.Models.Input;
using Khepri.Types;

namespace Khepri.Entities.UnitComponents.States
{
    /// <summary> The unit is standing idle, waiting for an action. </summary>
    public class IdleState : UnitState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "idle_";

        /// <inheritdoc/>
        protected override Type[] _connectingStates { get; } =
        [
            typeof(WalkingState),
            typeof(SprintingState)
        ];


        /// <summary> The unit is standing idle, waiting for an action. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public IdleState(Unit unit) : base(unit) { }


        /// <inheritdoc/>
        public override void Update(Double delta)
        {
            _unit.AnimatedSprite.TransitionAnimation(this, _unit.Direction.ToDirection());

            // TODO - Have a timer here to do a fidget animation.
            // Apply gravity if we're not on the ground.
            if (!_unit.IsOnFloor())
            {
                _unit.Velocity = new Vector3(0f, 9.81f, 0f) * 0.5f * (Single)delta;
                _unit.MoveAndSlide();
            }
        }


        /// <inheritdoc/>
        public override void HandleInput(IInput input)
        {
            if (input is MoveInput move)
            {
                switch (move.MovementType)
                {
                    case MoveType.WALKING:
                        _unit.TrySetUnitState(typeof(WalkingState));
                        break;
                    case MoveType.SPRINTING:
                        _unit.TrySetUnitState(typeof(SprintingState));
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
