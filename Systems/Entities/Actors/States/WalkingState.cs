using System;
using Godot;
using Khepri.Resources.Actors;
using Khepri.Types;

namespace Khepri.Entities.Actors.States
{
    /// <summary> The being is walking across the ground. </summary>
    public class WalkingState : ActorState
    {
        /// <inheritdoc/>
        public override String AnimationPrefix { get; } = "Walk_";


        /// <summary> The being is walking across the ground. </summary>
        /// <param name="being"> A reference to the being. </param>
        public WalkingState(ActorNode being) : base(being) { }


        /// <inheritdoc/>
        public override void HandleInput(IInput input)
        {
            if (input is MoveInput move)
            {
                _being.Velocity = move.Direction * _being.GetResource<BeingResource>().BaseSpeed;
                _being.Sprite.TransitionAnimation(this, move.Direction.ToDirection());
            }
        }


        /// <inheritdoc/>
        public override void Update(Double delta)
        {
            // Apply gravity if we're not on the ground.
            if (!_being.IsOnFloor())
            {
                _being.Velocity -= new Vector3(0f, 9.81f, 0f) * 0.5f * (Single)delta;
            }

            _being.MoveAndSlide();
        }
    }
}
