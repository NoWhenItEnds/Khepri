using System;
using System.Linq;
using Khepri.Models.Input;

namespace Khepri.Entities.UnitStates
{
    /// <summary> The basic data object representing a unit's potential state. </summary>
    public abstract class UnitState
    {
        /// <summary> The prefix of the animations to use for this state. </summary>
        public abstract String AnimationPrefix { get; }

        /// <summary> An array of the states that this one can transition to. </summary>
        protected abstract Type[] _connectingStates { get; }

        /// <summary> A reference to the unit. </summary>
        protected readonly Unit _unit;


        /// <summary> The basic data object representing a unit's potential state. </summary>
        /// <param name="unit"> A reference to the unit. </param>
        public UnitState(Unit unit)
        {
            _unit = unit;
        }


        /// <summary> Update the unit state. Called on the physics frame. </summary>
        /// <param name="delta"> The time in second since the last physics frame. </param>
        public abstract void Update(Double delta);


        /// <summary> Handle input during the current unit's state. </summary>
        /// <param name="input"> The input data object. </param>
        public abstract void HandleInput(IInput input);


        /// <summary> Attempt to transition the state to the given one. </summary>
        /// <param name="state"> The new state to transition to. </param>
        /// <returns> The resulting state. Either the current if a transition wasn't possible, or the newly created one. </returns>
        public UnitState TryTransitionTo(Type state)
        {
            UnitState result = this;
            if (CanTransitionTo(state))
            {
                Cleanup();
                result = Activator.CreateInstance(state, new object[] { _unit }) as UnitState;
                result.Initialise();
            }
            return result;
        }


        /// <summary> Check if the current state can transition to a given one.  </summary>
        /// <param name="state"> The state to check a transition to. </param>
        /// <returns> Whether the state can CURRENTLY transition to the provided state. </returns>
        protected Boolean CanTransitionTo(Type state)
        {
            return _connectingStates.Contains(state);
        }


        /// <summary> Initialise the unit state. Called once when the state is created. </summary>
        protected abstract void Initialise();


        /// <summary> Called just before the state transitions. Does a final cleanup. </summary>
        protected abstract void Cleanup();
    }
}
