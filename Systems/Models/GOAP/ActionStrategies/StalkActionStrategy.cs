using Khepri.Entities;
using System;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Tail someone. Stay hidden. Be creepy. </summary>
    public class StalkActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => !IsComplete;

        /// <inheritdoc/>
        public Boolean IsComplete => throw new NotImplementedException();

        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> A reference to the target being stalked. </summary>
        private readonly Unit _target;


        /// <summary> Tail someone. Stay hidden. Be creepy. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="target"> A reference to the target being stalked. </param>
        public StalkActionStrategy(Unit unit, Unit target)
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
