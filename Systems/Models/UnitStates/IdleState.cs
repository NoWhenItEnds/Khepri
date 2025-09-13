using Godot;
using System;

namespace Khepri.Models.UnitStates
{
    /// <summary> The unit is standing idle, waiting for an action. </summary>
    public class IdleState : IUnitState
    {
        /// <inheritdoc/>
        public String AnimationPrefix { get; private set; } = "idle_";
    }
}
