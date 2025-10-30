using Godot;
using Khepri.Data.Items;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Attempt to pickup an item. </summary>
    public partial class PickupActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _unit.UsableEntities.Where(x => x is ItemNode item && item.DataKind == _itemKind).Count() > 0;

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly ActorNode _unit;

        /// <summary> The desired item's common name or kind. </summary>
        private readonly String _itemKind;


        /// <summary> Attempt to pickup an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="kind"> The desired item's common name or kind. </param>
        public PickupActionStrategy(ActorNode unit, String kind)
        {
            _unit = unit;
            _itemKind = kind;
        }


        /// <inheritdoc/>
        public void Start()
        {
            IEnumerable<ItemNode> items = _unit.UsableEntities.Where(x => x is ItemNode).Cast<ItemNode>();
            if (items.Count() > 0)      // If the unit is close enough to usable items.
            {
                ItemNode? item = items.FirstOrDefault(x => x.DataKind == _itemKind);

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
