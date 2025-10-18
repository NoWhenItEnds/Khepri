using System;
using System.Linq;
using Godot;
using Khepri.Entities;
using Khepri.Entities.Actors;

namespace Khepri.GOAP.ActionStrategies
{
    /// <summary> Wander randomly. Take in the scenery. </summary>
    public class WanderActionStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public bool IsValid => !IsComplete; // TODO - See if this is necessary.

        /// <inheritdoc/>
        public bool IsComplete => _unit.NavigationAgent.IsNavigationFinished();

        /// <summary> A reference to the random class used by the strategy. </summary>
        private readonly Random _random = new Random();

        /// <summary> A reference to the unit being manipulated. </summary>
        private readonly ActorNode _unit;

        /// <summary> The radius to choose a point to wander towards. </summary>
        private readonly Single _radius;


        /// <summary> Wander randomly. Take in the scenery. </summary>
        /// <param name="unit"> A reference to the unit being manipulated. </param>
        /// <param name="radius"> The radius to choose a point to wander towards. </param>
        public WanderActionStrategy(ActorNode unit, Single radius)
        {
            _unit = unit;
            _radius = radius;
        }


        /// <inheritdoc/>
        public void Start()
        {
            // We want to choose a position somewhat distant to the unit as our start point.
            Vector3[] knownPositions = _unit.Sensors.GetKnownPositions();
            Vector3[] flitteredPositions = knownPositions
                .OrderByDescending(x => x.DistanceTo(_unit.GlobalPosition))
                .Take((Int32)Math.Ceiling(knownPositions.Length / 2f)).ToArray();   // Take the more distant points.
            Vector3 chosenPosition = flitteredPositions[_random.Next(flitteredPositions.Length)];
            for (Int32 i = 0; i < 100; i++)   // Attempt "several" times to find a reachable location.
            {
                Single xPos = _random.NextSingle() * _radius * 2f - _radius;
                Single zPos = _random.NextSingle() * _radius * 2f - _radius;
                Vector3 targetPosition = chosenPosition + new Vector3(xPos, 0f, zPos);

                // Check the current level.
                _unit.NavigationAgent.TargetPosition = targetPosition;
                if (_unit.NavigationAgent.IsTargetReachable()) { break; }

                // Check the upper level.
                _unit.NavigationAgent.TargetPosition = targetPosition + Vector3.Up;
                if (_unit.NavigationAgent.IsTargetReachable()) { break; }

                // Check the lower level.
                _unit.NavigationAgent.TargetPosition = targetPosition + Vector3.Down;
                if (_unit.NavigationAgent.IsTargetReachable()) { break; }

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
