using Khepri.Data.Actors;
using Khepri.Data.Items;
using Khepri.Entities.Actors;
using Khepri.Types.Exceptions;
using System;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Use an item in a unit's inventory. </summary>
    public partial class UseInventoryItemActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _unit.GetData<BeingData>().Inventory.HasItem(_itemKind) > 0;

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly ActorNode _unit;

        /// <summary> The item's name or kind. </summary>
        private readonly String _itemKind;


        /// <summary> Use an item in a unit's inventory. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemKind"> The desired item's name or kind. </param>
        public UseInventoryItemActionStrategy(ActorNode unit, String itemKind)
        {
            _unit = unit;
            _itemKind = itemKind;
        }


        /// <inheritdoc/>
        public void Start()
        {
            ItemData[] items = _unit.GetData<BeingData>().Inventory.GetItem(_itemKind);
            if (items.Length > 0)
            {
                ItemData item = items[0];
                item.Use(_unit);   // TODO - Move to being used by state machine i.e. HandleInput(); Figure out how to use items that are not IEntities.
                IsComplete = true;
            }
            else
            {
                throw new ActionStrategyException("Somehow the strategy found items that don't exist. WTF.");
            }
        }


        /// <inheritdoc/>
        public void Update(Double delta) { }


        /// <inheritdoc/>
        public void Stop() { }
    }
}
