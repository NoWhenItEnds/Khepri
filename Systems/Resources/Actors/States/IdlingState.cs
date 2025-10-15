using System;
using Godot;
using Khepri.Types;

namespace Khepri.Entities.Actors.Components.States
{
    /// <summary> The being is standing idle, waiting for an action. </summary>
    public class IdlingState : BeingState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "Idle_";


        /// <summary> The being is standing idle, waiting for an action. </summary>
        /// <param name="being"> A reference to the being. </param>
        public IdlingState(Being being) : base(being) { }


        /// <inheritdoc/>
        public override void HandleInput(IInput input) { }


        /// <inheritdoc/>
        public override void Update(Double delta)
        {
            // TODO - Have a timer here to do a fidget animation.
            _being.Sprite.TransitionAnimation(this, _being.Direction.ToDirection());

            _being.Velocity = new Vector3(0f, 0f, 0f);

            // Apply gravity if we're not on the ground.
            if (!_being.IsOnFloor())
            {
                _being.Velocity -= new Vector3(0f, 9.81f, 0f) * 0.5f * (Single)delta;
            }

            _being.MoveAndSlide();
        }
    }
}
