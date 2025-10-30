using Godot;
using Khepri.Data.Actors;
using Khepri.Entities;
using Khepri.Entities.Actors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.GOAP
{
    /// <summary> Construct beliefs on an industrial scale. </summary>
    public class BeliefFactory
    {
        /// <summary> A reference to the being who the beliefs are associated with. </summary>
        private readonly ActorNode _being;

        /// <summary> The array of current beliefs. </summary>
        private readonly Dictionary<String, AgentBelief> _beliefs;


        /// <summary> Construct beliefs on an industrial scale. </summary>
        /// <param name="being"> A reference to the decision-making agent. </param>
        /// <param name="beliefs"> The array of current beliefs. </param>
        public BeliefFactory(ActorNode being, Dictionary<String, AgentBelief> beliefs)
        {
            _being = being;
            _beliefs = beliefs;
        }


        /// <summary> Adds a new belief with a simple boolean conditional. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="condition"> The function the belief uses to evaluate the nature of the condition. </param>
        public void AddBelief(String key, Func<Boolean> condition)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(condition)
                .Build());
        }


        /// <summary> Adds a belief about a kind of entity stored in the being's sensors. </summary>
        /// <typeparam name="T"> The kind of node to search for. </typeparam>
        /// <param name="key"> The name of belief. </param>
        /// <param name="kind"> The common identifying name or key of the entity. </param>
        public void AddEntityBelief<T>(String key, String kind) where T : IEntity
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => _being.Sensors.TryGetEntity<T>()
                    .FirstOrDefault(x => x.Entity.DataKind == kind) != null)
                .Build());
        }


        /// <summary> Adds a belief about whether the agent is currently within interaction range of a kind of node. </summary>
        /// <typeparam name="T"> The kind of node to search for. </typeparam>
        /// <param name="key"> The name of belief. </param>
        /// <param name="kind"> The common identifying name or key of the entity. </param>
        /// <param name="distance"> The acceptable distance or range from the location. </param>
        public void AddNodeLocationBelief<T>(String key, String kind, Single distance) where T : IEntity
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => InRangeOf(_being.Sensors.TryGetEntity<T>()
                    .FirstOrDefault(x => x.Entity.DataKind == kind)?.LastKnownPosition, distance))
                .Build());
        }


        /// <summary> Add a new locational belief that requires the unit to be in range of a location. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="distance"> The acceptable distance or range from the location. </param>
        /// <param name="targetLocation"> The target position. </param>
        public void AddLocationBelief(String key, Single distance, Vector3 targetLocation)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => InRangeOf(targetLocation, distance))
                .Build());
        }


        /// <summary> Adds a belief about a kind of item stored in the being's inventory. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="kind"> The unique identifying name or key of the item. </param>
        public void AddInventoryBelief(String key, String kind)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => _being.GetData<BeingData>().Inventory.HasItem(kind) > 0)
                .Build());
        }


        /// <summary> Checks whether the given target location is within range of the unit. </summary>
        /// <param name="target"> The target position. </param>
        /// <param name="range"> The acceptable range. </param>
        /// <returns> If the unit is within acceptable range of the target location. </returns>
        private Boolean InRangeOf(Vector3? target, Single range) => target != null ? _being.GlobalPosition.DistanceTo(target.Value) < range : false;
    }


    /// <summary> A piece of knowledge the agent has about the world. </summary>
    public class AgentBelief : IEquatable<AgentBelief>
    {
        /// <summary> The identifying name or key of the belief. </summary>
        public String Name { get; private set; }

        /// <summary> The functions the belief uses to evaluate the nature of the condition. </summary>
        private List<Func<Boolean>> _conditions = new List<Func<Boolean>>();


        /// <summary> The identifying name or key of the belief. </summary>
        /// <param name="name"> The identifying name or key of the belief. </param>
        private AgentBelief(String name)
        {
            Name = name;
        }


        /// <summary> Calculate the condition to find out if the belief is true. </summary>
        /// <returns> Evaluates the belief to see if it is true or not. </returns>
        public Boolean Evaluate()
        {
            Boolean result = false;

            foreach (Func<Boolean> condition in _conditions)
            {
                result = condition();
                if (!result) { break; }
            }

            return result;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            AgentBelief? other = obj as AgentBelief;
            return other != null ? Name.Equals(other.Name) : false;
        }


        /// <inheritdoc/>
        public bool Equals(AgentBelief? other) => Name.Equals(other?.Name);


        /// <summary> A builder for creating and modifying beliefs. </summary>
        public class Builder
        {
            /// <summary> The belief the builder is associated with. </summary>
            private readonly AgentBelief _belief;


            /// <summary> A builder for creating and modifying beliefs. </summary>
            /// <param name="name"> The identifying name or key of the belief. </param>
            public Builder(String name)
            {
                _belief = new AgentBelief(name);
            }


            /// <summary> Add a condition to the belief. </summary>
            /// <param name="condition"> The delegate used to evaluate the condition. </param>
            public Builder WithCondition(Func<Boolean> condition)
            {
                _belief._conditions.Add(condition);
                return this;
            }


            /// <summary> Build the architected belief. </summary>
            /// <returns> The newly constructed belief. </returns>
            public AgentBelief Build()
            {
                return _belief;
            }
        }
    }
}
