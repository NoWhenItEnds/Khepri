using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Actors.Components.States
{
    /// <summary> An event that triggers a transition in the state machine. </summary>
    public enum StateEvent
    {
        NONE,
        IDLE,
        WALK,
        SPRINT
    }


    /// <summary> A state machine for unit states. </summary>
    public class UnitStateMachine
    {
        /// <summary> The current state of the state machine. </summary>
        public UnitState CurrentState { get; private set; }


        /// <summary> The default state to transition to if the state machine finds an invalid value. </summary>
        private UnitState _defaultState;

        /// <summary> All the possible states the state machine can transition to. </summary>
        private HashSet<UnitState> _states = new HashSet<UnitState>();


        /// <summary> A state machine for unit states. </summary>
        /// <param name="unit"> The unit controlled by the state machine. </param>
        public UnitStateMachine(Unit unit)
        {
            // Set the default state.
            _defaultState = new IdlingState(unit)
                .WithTransition<WalkingState>(StateEvent.WALK)
                .WithTransition<SprintingState>(StateEvent.SPRINT);

            CurrentState = _defaultState;

            _states.Add(_defaultState);

            _states.Add(new WalkingState(unit)
                .WithTransition<IdlingState>(StateEvent.IDLE)
                .WithTransition<SprintingState>(StateEvent.SPRINT));

            _states.Add(new SprintingState(unit)
                .WithTransition<IdlingState>(StateEvent.IDLE)
                .WithTransition<WalkingState>(StateEvent.WALK));
        }


        /// <summary> Transition from one state to another. </summary>
        /// <param name="stateEvent"> The event triggering the state change. </param>
        public void TransitionState(StateEvent stateEvent)
        {
            Type? nextState = CurrentState.GetNextState(stateEvent);
            if (nextState != null)
            {
                CurrentState.Stop();
                CurrentState = _states.FirstOrDefault(x => x.GetType() == nextState) ?? _defaultState;
                CurrentState.Start();
            }
        }
    }
}
