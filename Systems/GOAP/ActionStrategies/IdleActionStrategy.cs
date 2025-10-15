using System;
using Khepri.Entities.Actors;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Stand around. Look pretty. </summary>
    public class IdleActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid { get; private set; } = true;    // We can always retardmax.

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Being _unit;

        /// <summary> How long the action has remaining before it's complete. </summary>
        private Single _duration;

        /// <summary> The current amount of time remaining before the action is complete. </summary>
        private Single _currentTimeRemaining;


        /// <summary> Stand around. Look pretty. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="duration"> How long the action has remaining before it's complete. </param>
        public IdleActionStrategy(Being unit, Single duration)
        {
            _unit = unit;   // TODO - This should play an idle animation.
            _duration = duration;
        }


        /// <inheritdoc/>
        public void Start()
        {
            _currentTimeRemaining = _duration;
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
            _currentTimeRemaining -= (Single)delta;
            IsComplete = _currentTimeRemaining <= 0f;
        }


        /// <inheritdoc/>
        public void Stop() { }
    }
}
