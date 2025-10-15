using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Khepri.Entities.Items;

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


    /// <summary> A state machine for being states. </summary>
    public partial class BeingStateMachine : Resource
    {
        /// <summary> A reference to the being controlled by the state. </summary>
        private Being _being;

        /// <summary> The current state of the state machine. </summary>
        public BeingState CurrentState { get; private set; }


        /// <summary> The default state to transition to if the state machine finds an invalid value. </summary>
        private BeingState _defaultState;

        /// <summary> All the possible states the state machine can transition to. </summary>
        private HashSet<BeingState> _states = new HashSet<BeingState>();


        /// <summary> A state machine for being states. </summary>
        public BeingStateMachine() { }


        /// <summary> Initialise the resource. </summary>
        /// <param name="being"> A reference to the being controlled by the state. </param>
        public void Initialise(Being being)
        {
            _being = being;

            // Set the default state.
            _defaultState = new IdlingState(_being)
                .WithTransition<WalkingState>(StateEvent.WALK)
                .WithTransition<SprintingState>(StateEvent.SPRINT);

            CurrentState = _defaultState;

            _states.Add(_defaultState);

            _states.Add(new WalkingState(_being)
                .WithTransition<IdlingState>(StateEvent.IDLE)
                .WithTransition<SprintingState>(StateEvent.SPRINT));

            _states.Add(new SprintingState(_being)
                .WithTransition<IdlingState>(StateEvent.IDLE)
                .WithTransition<WalkingState>(StateEvent.WALK));
        }


        /// <summary> Handle the input sent to the being by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input)
        {
            if (input is MoveInput moveInput)
            {
                switch (moveInput.MovementType)
                {
                    case MoveInput.MoveType.WALKING:
                        TransitionState(StateEvent.WALK);
                        break;
                    case MoveInput.MoveType.SPRINTING:
                        TransitionState(StateEvent.SPRINT);
                        break;
                    case MoveInput.MoveType.IDLE:
                    default:
                        TransitionState(StateEvent.IDLE);
                        break;
                }
            }
            else if (input is ExamineInput examineInput)    // TODO - State change?
            {
                examineInput.Entity.Examine(_being);
            }
            else if (input is UseInput useInput)
            {
                useInput.Entity.Use(_being);
            }
            else if (input is GrabInput grabInput)
            {
                if (grabInput.Entity is ItemNode item)
                {
                    item.Grab(_being);
                }
            }

            CurrentState.HandleInput(input);
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


        /// <summary> Update the current state every frame. </summary>
        /// <param name="delta"> The time since the previous frame. </param>
        public void Update(Double delta)
        {
            CurrentState.Update(delta);
        }
    }
}
