using Godot;
using Khepri.Entities;
using Khepri.Entities.Interfaces;
using Khepri.Entities.UnitComponents;
using Khepri.Models.Input;
using System;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Search for a specific entity. </summary>
    public class LocateActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => !IsComplete;

        /// <inheritdoc/>
        public Boolean IsComplete => _unit.NavigationAgent.IsNavigationFinished();

        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> A reference to the target being searched for. </summary>
        private readonly IEntity _target;


        /// <summary> Search for a specific entity. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="target"> A reference to the target being searched for. </param>
        public LocateActionStrategy(Unit unit, IEntity target)
        {
            _unit = unit;
            _target = target;
        }


        /// <inheritdoc/>
        public void Start()
        {
            KnownEntity? entity = _unit.Brain.KnowsEntity(_target);
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
