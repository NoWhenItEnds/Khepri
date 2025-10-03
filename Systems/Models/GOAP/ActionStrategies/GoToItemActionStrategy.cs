using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Actors.Components;
using Khepri.Models.Input;
using System;
using System.Linq;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Go to the know location of an item. </summary>
    public partial class GoToItemActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _itemName != null ? _unit.Sensors.TryGetItem(_itemName).Length > 0 : _unit.Sensors.TryGetItem(_itemId.Value) != null;

        /// <inheritdoc/>
        public Boolean IsComplete => _unit.NavigationAgent.IsNavigationFinished();


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> The desired item's name or kind. </summary>
        /// <remarks> This is one way an item can be identified. If this is null, the other value will be used. </remarks>
        private readonly String? _itemName = null;

        /// <summary> The unique identifier of the specific item desired. </summary>
        /// <remarks> This is one way an item can be identified. If this is null, the other value will be used. </remarks>
        private readonly Guid? _itemId = null;


        /// <summary> Go to the know location of an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemName"> The desired item's name or kind. </param>
        public GoToItemActionStrategy(Unit unit, String itemName)
        {
            _unit = unit;
            _itemName = itemName;
        }


        /// <summary> Go to the know location of an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemId"> The unique identifier of the specific item desired. </param>
        public GoToItemActionStrategy(Unit unit, Guid itemId)
        {
            _unit = unit;
            _itemId = itemId;
        }


        /// <inheritdoc/>
        public void Start()
        {
            KnownEntity? entity = null;

            if (_itemName != null)  // If we're using the item name, try to find a known item of the same kind.
            {
                KnownEntity[] items = _unit.Sensors.TryGetItem(_itemName);
                if (items.Length > 0)
                {
                    entity = items.OrderBy(x => _unit.GlobalPosition.DistanceTo(x.LastKnownPosition)).FirstOrDefault();
                }
            }
            else                    // Else, try to find the specific item.
            {
                entity = _unit.Sensors.TryGetItem(_itemId.Value);
            }

            // If an entity to go to was found.
            if (entity != null)
            {
                _unit.NavigationAgent.TargetPosition = entity.LastKnownPosition;
            }
            else
            {
                _unit.NavigationAgent.TargetPosition = _unit.GlobalPosition;
            }
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
            if (!_unit.NavigationAgent.IsNavigationFinished())
            {
                Vector3 nextPosition = _unit.NavigationAgent.GetNextPathPosition();
                Vector3 direction = _unit.GlobalPosition.DirectionTo(nextPosition).Normalized();
                _unit.HandleInput(new MoveInput(direction, MoveType.WALKING));
            }
        }


        /// <inheritdoc/>
        public void Stop()
        {
            _unit.HandleInput(new MoveInput(Vector3.Zero, MoveType.IDLE));
        }
    }
}
