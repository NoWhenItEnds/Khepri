using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using Khepri.Models.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Attempt to pickup an item. </summary>
    public partial class PickupActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _itemName != null ?
            _unit.UsableEntities.Where(x => x is Item item && item.Data.Name == _itemName).Count() > 0 :
            _unit.UsableEntities.Where(x => x is Item item && item.Data.UId == _itemId).FirstOrDefault() != null;

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> The desired item's name or kind. </summary>
        /// <remarks> This is one way an item can be identified. If this is null, the other value will be used. </remarks>
        private readonly String? _itemName = null;

        /// <summary> The unique identifier of the specific item desired. </summary>
        /// <remarks> This is one way an item can be identified. If this is null, the other value will be used. </remarks>
        private readonly Guid? _itemId = null;


        /// <summary> Attempt to pickup an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemName"> The desired item's name or kind. </param>
        public PickupActionStrategy(Unit unit, String itemName)
        {
            _unit = unit;
            _itemName = itemName;
        }


        /// <summary> Attempt to pickup an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemId"> The unique identifier of the specific item desired. </param>
        public PickupActionStrategy(Unit unit, Guid itemId)
        {
            _unit = unit;
            _itemId = itemId;
        }


        /// <inheritdoc/>
        public void Start()
        {
            IEnumerable<Item> items = _unit.UsableEntities.Where(x => x is Item).Cast<Item>();
            if (items.Count() > 0)      // If the unit is close enough to usable items.
            {
                Item? item = null;
                if (_itemName != null)  // If we're using the item name, try to find a known item of the same kind.
                {
                    item = items.FirstOrDefault(x => x.Data.Name == _itemName);
                }
                else                    // Else, try to find the specific item.
                {
                    item = items.FirstOrDefault(x => x.Data.UId == _itemId);
                }

                if (item != null)  // If the item was actually found.
                {
                    _unit.HandleInput(new GrabInput(item)); // TODO - What if inventory is full? Make eat from ground?
                    IsComplete = true;
                }
            }
        }


        /// <inheritdoc/>
        public void Update(Double delta) { }


        /// <inheritdoc/>
        public void Stop() { }
    }
}
