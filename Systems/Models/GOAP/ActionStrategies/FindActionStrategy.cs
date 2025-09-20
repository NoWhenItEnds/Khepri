using Khepri.Entities;
using Khepri.Entities.Interfaces;
using System;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Search for an locate an entity of the given type. </summary>
    public class FindActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => !IsComplete;

        /// <inheritdoc/>
        public Boolean IsComplete => throw new NotImplementedException();

        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> A reference to the type of entity being searched for. </summary>
        private readonly Type _target;


        /// <summary> Search for an locate an entity of the given type. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="target"> A reference to the type of entity being searched for.  </param>
        public FindActionStrategy(Unit unit, Type target)
        {
            _unit = unit;
            _target = target;
        }


        /// <inheritdoc/>
        public void Start()
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Update(double delta)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
