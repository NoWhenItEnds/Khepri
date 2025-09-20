using Godot;
using Khepri.Entities;
using Khepri.Entities.Interfaces;
using System;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Search for a specific entity. </summary>
    public class LocateActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => !IsComplete;

        /// <inheritdoc/>
        public Boolean IsComplete => throw new NotImplementedException();

        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> A reference to the target being searched for. </summary>
        private readonly ISmartEntity _target;


        /// <summary> Search for a specific entity. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="target"> A reference to the target being searched for. </param>
        public LocateActionStrategy(Unit unit, ISmartEntity target)
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
