using Khepri.Entities.Actors;
using Khepri.Resources.Actors;
using System;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Use a device that is in the game world. </summary>
    public partial class UseDeviceNodeActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _unit.GetResource<BeingResource>().Inventory.HasItem(_deviceKind) > 0;

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly ActorNode _unit;

        /// <summary> The device's name or kind. </summary>
        private readonly String _deviceKind;


        /// <summary> Use an item in a unit's inventory. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="deviceKind"> The desired device's name or kind. </param>
        public UseDeviceNodeActionStrategy(ActorNode unit, String deviceKind)
        {
            _unit = unit;
            _deviceKind = deviceKind;
        }


        /// <inheritdoc/>
        public void Start()
        {
            // TODO - Use known devices.
        }


        /// <inheritdoc/>
        public void Update(Double delta) { }


        /// <inheritdoc/>
        public void Stop() { }
    }
}
