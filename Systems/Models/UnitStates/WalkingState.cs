using System;

namespace Khepri.Models.UnitStates
{
    /// <summary> The unit is walking across the ground. </summary>
    public class WalkingState : IUnitState
    {
        /// <inheritdoc/>
        public String AnimationPrefix { get; private set; } = "walking_";
    }
}
