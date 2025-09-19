using System;
using Godot;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Models.Input;

namespace Khepri.Models.GOAP.ActionStrategies
{
    /// <summary> Wander randomly. Take in the scenery. </summary>
    public class WanderActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public bool IsValid => !IsComplete;

        /// <inheritdoc/>
        public bool IsComplete => _unit.NavigationAgent.IsNavigationFinished();

        /// <summary> A reference to the random class used by the strategy. </summary>
        private readonly Random _random = new Random();

        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly Unit _unit;

        /// <summary> The radius to choose a point to wander towards. </summary>
        private readonly Single _radius;


        /// <summary> Wander randomly. Take in the scenery. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="radius"> The radius to choose a point to wander towards. </param>
        public WanderActionStrategy(Unit unit, Single radius)
        {
            _unit = unit;
            _radius = radius;
        }


        /// <inheritdoc/>
        public void Start()
        {
            for (Int32 i = 0; i < 100; i++)   // Attempt "several" times to find a reachable location.
            {
                Single xPos = _random.NextSingle() * _radius * 2f - _radius;
                Single zPos = _random.NextSingle() * _radius * 2f - _radius;
                Vector3 targetPosition = _unit.GlobalPosition + new Vector3(xPos, 0f, zPos);
                _unit.NavigationAgent.TargetPosition = targetPosition;
                if (_unit.NavigationAgent.IsTargetReachable())  // If we find one, stop trying.
                {
                    break;
                }
                _unit.NavigationAgent.TargetPosition = _unit.GlobalPosition;    // Else, reset it.
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
