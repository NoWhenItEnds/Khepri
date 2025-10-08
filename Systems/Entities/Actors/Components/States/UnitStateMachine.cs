using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Khepri.Entities.Items;
using Khepri.Models.Input;

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
    public partial class UnitStateMachine : Node
    {
        /// <summary> A reference to the unit controlled by the state. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _unit;

        /// <summary> The current state of the state machine. </summary>
        public UnitState CurrentState { get; private set; }


        /// <summary> The default state to transition to if the state machine finds an invalid value. </summary>
        private UnitState _defaultState;

        /// <summary> All the possible states the state machine can transition to. </summary>
        private HashSet<UnitState> _states = new HashSet<UnitState>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            // Set the default state.
            _defaultState = new IdlingState(_unit)
                .WithTransition<WalkingState>(StateEvent.WALK)
                .WithTransition<SprintingState>(StateEvent.SPRINT);

            CurrentState = _defaultState;

            _states.Add(_defaultState);

            _states.Add(new WalkingState(_unit)
                .WithTransition<IdlingState>(StateEvent.IDLE)
                .WithTransition<SprintingState>(StateEvent.SPRINT));

            _states.Add(new SprintingState(_unit)
                .WithTransition<IdlingState>(StateEvent.IDLE)
                .WithTransition<WalkingState>(StateEvent.WALK));
        }


        /// <summary> Handle the input sent to the unit by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input)
        {
            if (input is MoveInput moveInput)
            {
                switch (moveInput.MovementType)
                {
                    case MoveType.WALKING:
                        TransitionState(StateEvent.WALK);
                        break;
                    case MoveType.SPRINTING:
                        TransitionState(StateEvent.SPRINT);
                        break;
                    case MoveType.IDLE:
                    default:
                        TransitionState(StateEvent.IDLE);
                        break;
                }
            }
            else if (input is ExamineInput examineInput)    // TODO - State change?
            {
                examineInput.Entity.Examine(_unit);
            }
            else if (input is UseInput useInput)
            {
                useInput.Entity.Use(_unit);
            }
            else if (input is GrabInput grabInput)
            {
                if (grabInput.Entity is ItemNode item)
                {
                    item.Grab(_unit);
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


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            CurrentState.Update(delta);
        }
    }
}
