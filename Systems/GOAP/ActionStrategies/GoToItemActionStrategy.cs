using Godot;
using Khepri.Data.Items;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Entities.Actors.Components;
using Khepri.Entities.Items;
using System;
using System.Linq;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Go to the know location of an item. </summary>
    public partial class GoToItemActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _unit.Sensors.TryGetEntity<ItemNode>().FirstOrDefault(x => x.Entity.DataKind == _itemKind) != null;

        /// <inheritdoc/>
        public Boolean IsComplete => _unit.NavigationAgent.IsNavigationFinished();


        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly ActorNode _unit;

        /// <summary> The desired item's common type or kind. </summary>
        private readonly String _itemKind;


        /// <summary> Go to the know location of an item. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="itemKind"> The desired item's common type or kind. </param>
        public GoToItemActionStrategy(ActorNode unit, String itemKind)
        {
            _unit = unit;
            _itemKind = itemKind;
        }


        /// <inheritdoc/>
        public void Start()
        {
            KnownEntity[] items = _unit.Sensors.TryGetEntity<ItemNode>().Where(x => x.Entity.DataKind == _itemKind).ToArray();
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
                _unit.HandleInput(new MoveInput(direction, MoveInput.MoveType.WALKING));
            }
        }


        /// <inheritdoc/>
        public void Stop()
        {
            _unit.HandleInput(new MoveInput(Vector3.Zero, MoveInput.MoveType.IDLE));
        }
    }
}
