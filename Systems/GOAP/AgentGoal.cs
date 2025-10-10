using System;
using System.Collections.Generic;

namespace Khepri.GOAP
{
    /// <summary> A goal that the agent desires and will use a chain of actions to complete. </summary>
    /// <remarks> Under the hood this is really just a series of desired states / beliefs. </remarks>
    public class AgentGoal : IEquatable<AgentGoal>
    {
        /// <summary> The name or key identifying the goal. </summary>
        public String Name { get; }

        /// <summary> The importance of the goal the agent. More important goals will be tackled before less. </summary>
        public Single Priority { get; private set; } = 1f;

        /// <summary> The desired state of the world for the goal to be considered complete. All actions should go towards addressing these outcomes. </summary>
        public HashSet<AgentBelief> DesiredOutcomes { get; } = new();


        /// <summary> A goal that the agent desires and will use a chain of actions to complete. </summary>
        /// <param name="name"> The name or key identifying the goal. </param>
        private AgentGoal(String name)
        {
            Name = name;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            AgentGoal? other = obj as AgentGoal;
            return other != null ? Name.Equals(other.Name) : false;
        }


        /// <inheritdoc/>
        public bool Equals(AgentGoal? other) => Name.Equals(other?.Name);


        /// <summary> A helpful builder that allows for easy construction of agent goals. </summary>
        public class Builder
        {
            /// <summary> A reference to the goal being constructed. </summary>
            private readonly AgentGoal _goal;


            /// <summary> A helpful builder that allows for easy construction of agent goals. </summary>
            /// <param name="name"> The name or key identifying the goal. </param>
            public Builder(String name)
            {
                _goal = new AgentGoal(name);
            }


            /// <summary> Sets the goal's priority. </summary>
            /// <param name="priority"> The importance of the goal the agent. More important goals will be tackled before less. </param>
            public Builder WithPriority(Single priority)
            {
                _goal.Priority = priority;
                return this;
            }


            /// <summary> Adds a desired outcome to the goal. </summary>
            /// <param name="outcome"> The desired state of the world for the goal to be considered complete. </param>
            public Builder WithDesiredOutcome(AgentBelief outcome)
            {
                _goal.DesiredOutcomes.Add(outcome);
                return this;
            }


            /// <summary> Adds a desired outcome to the goal. </summary>
            /// <param name="outcomes"> The desired states of the world for the goal to be considered complete. </param>
            public Builder WithDesiredOutcome(AgentBelief[] outcomes)
            {
                foreach (AgentBelief outcome in outcomes)
                {
                    _goal.DesiredOutcomes.Add(outcome);
                }
                return this;
            }


            /// <summary> Build the architected goal. </summary>
            /// <returns> The newly constructed goal. </returns>
            public AgentGoal Build()
            {
                return _goal;
            }
        }
    }
}
