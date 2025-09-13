using System;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Stand around. Look pretty. </summary>
    public class IdleActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid { get; private set; } = true;    // We can always retardmax.

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> How long the action has remaining before it's complete. </summary>
        private Single _timeRemaining;


        /// <summary> Stand around. Look pretty. </summary>
        /// <param name="duration"> How long the action has remaining before it's complete. </param>
        public IdleActionStrategy(Single duration)
        {
            _timeRemaining = duration;
        }


        /// <inheritdoc/>
        public void Start() { }


        /// <inheritdoc/>
        public void Update(Single delta)
        {
            _timeRemaining -= delta;
            IsComplete = _timeRemaining <= 0f;
        }


        /// <inheritdoc/>
        public void Stop() { }
    }
}
