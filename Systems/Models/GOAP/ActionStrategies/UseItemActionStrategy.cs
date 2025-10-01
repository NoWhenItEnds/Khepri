using Godot;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Use an item in a unit's inventory. </summary>
    public partial class UseItemActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _itemName != null ? _unit.Inventory.HasItem(_itemName) > 0 : _unit.Inventory.HasItem(_itemId.Value);

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> The item's name or kind. </summary>
        /// <remarks> This is one way an item can be identified. If this is null, the other value will be used. </remarks>
        private readonly String? _itemName = null;

        /// <summary> The unique identifier of the specific item. </summary>
        /// <remarks> This is one way an item can be identified. If this is null, the other value will be used. </remarks>
        private readonly Guid? _itemId = null;


        /// <summary> Use an item in a unit's inventory. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemName"> The desired item's name or kind. </param>
        public UseItemActionStrategy(Unit unit, String itemName)
        {
            _unit = unit;
            _itemName = itemName;
        }


        /// <summary> Use an item in a unit's inventory. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemId"> The unique identifier of the specific item desired. </param>
        public UseItemActionStrategy(Unit unit, Guid itemId)
        {
            _unit = unit;
            _itemId = itemId;
        }


        /// <inheritdoc/>
        public void Start()
        {
            if (_itemName != null)  // If we're using the item name, try to find a known item of the same kind.
            {
                // TODO - Have use.
            }
            else                    // Else, try to find the specific item.
            {
                // TODO - Have use.
            }

            IsComplete = true;
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
        }


        /// <inheritdoc/>
        public void Stop()
        {
        }
    }
}
