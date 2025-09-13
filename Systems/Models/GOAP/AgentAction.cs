using System;
using System.Collections.Generic;
using Khepri.Models.GOAP.ActionStrategies;

namespace Khepri.Models.GOAP
{
    /// <summary> A potential action an entity can use to try to address a goal. </summary>
    public class AgentAction
    {
        /// <summary> The name or key identifying the action. </summary>
        public String Name { get; }

        /// <summary> How many 'action points' the action would cost to action. </summary>
        public Single Cost { get; private set; } = 1f;

        /// <summary> The beliefs or conditions that need to be true for the action to be actioned. </summary>
        public HashSet<AgentBelief> Preconditions { get; } = new HashSet<AgentBelief>();

        /// <summary> How the agent's beliefs or state will change as a result of the action. </summary>
        public HashSet<AgentBelief> Outcomes { get; } = new HashSet<AgentBelief>();

        /// <summary> Whether the action is now complete. </summary>
        public Boolean IsComplete => _strategy.IsComplete;


        /// <summary> A reference to the strategy / logic used for this action. </summary>
        private IActionStrategy _strategy;


        /// <summary> A potential action an entity can use to try to address a goal. </summary>
        /// <param name="name"> The name or key identifying the action. </param>
        private AgentAction(String name)
        {
            Name = name;
        }


        /// <summary> Begin carrying out the action. </summary>
        public void Start() => _strategy.Start();


        /// <summary> Update / incrementally run the current action. </summary>
        /// <param name="delta"> The time since the last update frame. </param>
        public void Update(Single delta)
        {
            if (_strategy.IsValid)
            {
                _strategy.Update(delta);
            }

            // Bail out if the strategy is still executing
            if (!_strategy.IsComplete) { return; }

            // Check to see if any outcomes have been met.
            foreach (AgentBelief outcome in Outcomes)
            {
                outcome.Evaluate();
            }
        }


        /// <summary> Stop or cancel the currently running action. Ensures this is done gracefully. </summary>
        public void Stop() => _strategy.Stop();


        /// <summary> A helpful builder that allows for easy construction of agent actions. </summary>
        public class AgentActionBuilder
        {
            /// <summary> A reference to the action being constructed. </summary>
            private readonly AgentAction _action;


            /// <summary> A helpful builder that allows for easy construction of agent actions. </summary>
            /// <param name="name"> The name or key identifying the action. </param>
            public AgentActionBuilder(String name)
            {
                _action = new AgentAction(name);
            }


            /// <summary> Sets the action cost. </summary>
            /// <param name="cost"> How many 'action points' the action would cost to action. </param>
            public AgentActionBuilder WithCost(Single cost)
            {
                _action.Cost = cost;
                return this;
            }


            /// <summary> Sets the action's strategy. </summary>
            /// <param name="strategy"> A reference to the strategy / logic used for this action. </param>
            public AgentActionBuilder WithStrategy(IActionStrategy strategy)
            {
                _action._strategy = strategy;
                return this;
            }


            /// <summary> Adds a precondition to the action that must be true for the action to begin. </summary>
            /// <param name="precondition"> The beliefs or conditions that need to be true for the action to be actioned. </param>
            public AgentActionBuilder AddPrecondition(AgentBelief precondition)
            {
                _action.Preconditions.Add(precondition);
                return this;
            }


            /// <summary> Adds preconditions to the action that must be true for the action to begin. </summary>
            /// <param name="preconditions"> The beliefs or conditions that need to be true for the action to be actioned. </param>
            public AgentActionBuilder AddPrecondition(AgentBelief[] preconditions)
            {
                foreach (AgentBelief precondition in preconditions)
                {
                    _action.Preconditions.Add(precondition);
                }
                return this;
            }


            /// <summary> Adds a outcome that is fulfilled by the action being completed. </summary>
            /// <param name="outcome"> How the agent's beliefs or state will change as a result of the action. </param>
            public AgentActionBuilder AddOutcome(AgentBelief outcome)
            {
                _action.Outcomes.Add(outcome);
                return this;
            }


            /// <summary> Adds outcomes that is fulfilled by the action being completed. </summary>
            /// <param name="outcomes"> How the agent's beliefs or state will change as a result of the action. </param>
            public AgentActionBuilder AddOutcome(AgentBelief[] outcomes)
            {
                foreach (AgentBelief outcome in outcomes)
                {
                    _action.Outcomes.Add(outcome);
                }
                return this;
            }


            /// <summary> Build the architected action. </summary>
            /// <returns> The newly constructed action. </returns>
            public AgentAction Build()
            {
                return _action;
            }
        }
    }
}
