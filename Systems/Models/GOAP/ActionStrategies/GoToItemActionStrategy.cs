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
        public Boolean IsValid => _unit.Sensors.TryGetItem(_itemKind).Length > 0;

        /// <inheritdoc/>
        public Boolean IsComplete => _unit.NavigationAgent.IsNavigationFinished();


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> The desired item's name or kind. </summary>
        private readonly String _itemKind;


        /// <summary> Go to the know location of an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemKind"> The desired item's name or kind. </param>
        public GoToItemActionStrategy(Unit unit, String itemKind)
        {
            _unit = unit;
            _itemKind = itemKind;
        }


        /// <inheritdoc/>
        public void Start()
        {
            KnownEntity[] items = _unit.Sensors.TryGetItem(_itemKind);
            if (items.Length > 0)
            {
                KnownEntity? entity = items.OrderBy(x => _unit.GlobalPosition.DistanceTo(x.LastKnownPosition)).FirstOrDefault();

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
