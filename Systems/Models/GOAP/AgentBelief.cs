using Godot;
using Khepri.Entities;
using Khepri.Entities.Interfaces;
using Khepri.Entities.Sensors;
using System;
using System.Collections.Generic;

namespace Khepri.Models.GOAP
{
    /// <summary> Construct beliefs on an industrial scale. </summary>
    public class BeliefFactory
    {
        /// <summary> A reference to the unit who the beliefs are associated with. </summary>
        private readonly Unit _unit;

        /// <summary> The array of current beliefs. </summary>
        private readonly Dictionary<String, AgentBelief> _beliefs;


        /// <summary> Construct beliefs on an industrial scale. </summary>
        /// <param name="unit"> A reference to the decision-making agent. </param>
        /// <param name="beliefs"> The array of current beliefs. </param>
        public BeliefFactory(Unit unit, Dictionary<String, AgentBelief> beliefs)
        {
            _unit = unit;
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


        /// <summary> Adds a new belief relating to information stored in an entity's sensors. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="sensor"> A reference to the sensor. </param>
        /// <param name="entity"> A reference to the entity we're concerned with. </param>
        public void AddSensorBelief(String key, UnitSensors sensor, ISmartEntity entity)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => sensor.KnowsEntity(entity) != null)
                .Build());
        }


        /// <summary> Adds a new belief relating to information stored in an entity's sensors. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="sensor"> A reference to the sensor. </param>
        /// <param name="entityType"> The type of entity we're concerned with. </param>
        public void AddSensorBelief(String key, UnitSensors sensor, Type entityType)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => sensor.KnowsEntityKind(entityType).Length > 0)
                .Build());
        }


        /// <summary> Add a new locational belief that requires the unit to be in range of a target node. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="distance"> The acceptable distance or range from the location. </param>
        /// <param name="targetNode"> The node that is being targeted. </param>
        public void AddLocationBelief(String key, Single distance, Node3D targetNode)
        {
            AddLocationBelief(key, distance, targetNode.GlobalPosition);
        }


        /// <summary> Add a new locational belief that requires the unit to be in range of a location. </summary>
        /// <param name="key"> The name of belief. </param>
        /// <param name="distance"> The acceptable distance or range from the location. </param>
        /// <param name="targetLocation"> The target position. </param>
        public void AddLocationBelief(String key, Single distance, Vector3 targetLocation)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key)
                .WithCondition(() => InRangeOf(targetLocation, distance))
                .WithLocation(() => targetLocation)
                .Build());
        }


        /// <summary> Checks whether the given target location is within range of the unit. </summary>
        /// <param name="target"> The target position. </param>
        /// <param name="range"> The acceptable range. </param>
        /// <returns> If the unit is within acceptable range of the target location. </returns>
        private Boolean InRangeOf(Vector3 target, Single range) => _unit.GlobalPosition.DistanceTo(target) < range;
    }


    /// <summary> A piece of knowledge the agent has about the world. </summary>
    public class AgentBelief
    {
        /// <summary> The identifying name or key of the belief. </summary>
        public String Name { get; private set; }

        /// <summary> The function the belief uses to evaluate the nature of the condition. </summary>
        private Func<Boolean> _condition = () => false;

        /// <summary> The function used to find locational information about the belief. </summary>
        private Func<Vector3> _observedLocation = () => Vector3.Zero;

        /// <summary> Returns locational information about the belief. </summary>
        public Vector3 Location => _observedLocation();


        /// <summary> The identifying name or key of the belief. </summary>
        /// <param name="name"> The identifying name or key of the belief. </param>
        private AgentBelief(String name)
        {
            Name = name;
        }


        /// <summary> Calculate the condition to find out if the belief is true. </summary>
        /// <returns> Evaluates the belief to see if it is true or not. </returns>
        public Boolean Evaluate() => _condition();


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
                _belief._condition = condition;
                return this;
            }


            /// <summary> Add an observed location to the belief. </summary>
            /// <param name="location"> The delegate used to evaluate the location. </param>
            public Builder WithLocation(Func<Vector3> location)
            {
                _belief._observedLocation = location;
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
