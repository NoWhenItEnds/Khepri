using System;
using Khepri.Entities;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Wander randomly. Take in the scenery. </summary>
    public class WanderActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public bool IsValid => !IsComplete;

        /// <inheritdoc/>
        public bool IsComplete => throw new NotImplementedException();


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> The radius to choose a point to wander towards. </summary>
        private readonly Single _radius;


        /// <summary> Wander randomly. Take in the scenery. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="radius"> The radius to choose a point to wander towards. </param>
        public WanderActionStrategy(Unit unit, Single radius)
        {
            _unit = unit;
            _radius = radius;
        }


        /// <inheritdoc/>
        public void Start()
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Update(Single delta)
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
