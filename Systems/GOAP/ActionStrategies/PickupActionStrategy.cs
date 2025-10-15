using Godot;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using Khepri.Resources.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Attempt to pickup an item. </summary>
    public partial class PickupActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _unit.UsableEntities.Where(x => x is ItemNode item && item.GetResource<ItemResource>().Id == _itemId).Count() > 0;

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Being _unit;

        /// <summary> The desired item's unique name. </summary>
        private readonly String _itemId;


        /// <summary> Attempt to pickup an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemId"> The desired item's unique name. </param>
        public PickupActionStrategy(Being unit, String itemId)
        {
            _unit = unit;
            _itemId = itemId;
        }


        /// <inheritdoc/>
        public void Start()
        {
            IEnumerable<ItemNode> items = _unit.UsableEntities.Where(x => x is ItemNode).Cast<ItemNode>();
            if (items.Count() > 0)      // If the unit is close enough to usable items.
            {
                ItemNode? item = items.FirstOrDefault(x => x.GetResource<ItemResource>().Id == _itemId);

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
