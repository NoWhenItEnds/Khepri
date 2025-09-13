using System;

namespace Khepri.Models.UnitStates
{
    /// <summary> The unit is moving quickly along the ground. </summary>
    public class SprintingState : IUnitState
    {
        /// <inheritdoc/>
        public String AnimationPrefix { get; private set; } = "sprinting_";
    }
}
