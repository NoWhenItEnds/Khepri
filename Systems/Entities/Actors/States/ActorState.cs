using System;
using System.Collections.Generic;

namespace Khepri.Entities.Actors.States
{
    /// <summary> The basic data object representing a being's potential state. </summary>
    public abstract class ActorState : IEquatable<ActorState>
    {
        /// <summary> The prefix of the animations to use for this state. </summary>
        public abstract String AnimationPrefix { get; }

        /// <summary> The states this state can transition into. </summary>
        protected Dictionary<StateEvent, Type> _transitions = new Dictionary<StateEvent, Type>();

        /// <summary> A reference to the being. </summary>
        protected readonly Being _being;


        /// <summary> The basic data object representing a being's potential state. </summary>
        /// <param name="being"> A reference to the being the state controls. </param>
        public ActorState(Being being)
        {
            _being = being;
        }


        /// <summary> Get the state that will result from the given triggering event. </summary>
        /// <param name="triggeringEvent"> The event triggering the state change. </param>
        /// <returns> The new resulting state. </returns>
        public Type? GetNextState(StateEvent triggeringEvent)
        {
            _transitions.TryGetValue(triggeringEvent, out Type? result);
            return result;
        }


        /// <summary> Adds a transition to the state. </summary>
        /// <typeparam name="T"> The state that will be transitioned to. </typeparam>
        /// <param name="stateEvent"> The triggering event. </param>
        /// <returns> The altered state. </returns>
        /// <exception cref="ArgumentException"> If the new transition couldn't be added to the state. </exception>
        public ActorState WithTransition<T>(StateEvent stateEvent) where T : ActorState
        {
            if (!_transitions.TryAdd(stateEvent, typeof(T)))
            {
                throw new ArgumentException("Unable to add the given transition to the being's state machine.", $"{stateEvent.GetType()}");
            }
            return this;
        }


        /// <summary> Update the being state. Called on the physics frame. </summary>
        /// <param name="delta"> The time in second since the last physics frame. </param>
        public abstract void Update(Double delta);


        /// <summary> Handle input during the current being's state. </summary>
        /// <param name="input"> The input data object. </param>
        public abstract void HandleInput(IInput input);


        /// <summary> Initialise the being state. Called once when the state is created. </summary>
        public virtual void Start() { }


        /// <summary> Called just before the state transitions. Does a final cleanup. </summary>
        public virtual void Stop() { }


        /// <inheritdoc/>
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(GetType());
        }


        /// <inheritdoc/>
        public Boolean Equals(ActorState? other)
        {
            return other != null ? GetType() == other.GetType() : false;
        }
    }
}
