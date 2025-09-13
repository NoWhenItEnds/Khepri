using System;

namespace Khepri.Models.UnitStates
{
    /// <summary> The basic data object representing a unit's potential state. </summary>
    public interface IUnitState
    {
        /// <summary> The prefix of the animations to use for this state. </summary>
        public abstract String AnimationPrefix { get; }
    }
}
