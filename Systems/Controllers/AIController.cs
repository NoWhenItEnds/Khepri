using Godot;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Entities.Sensors;
using Khepri.Models.Input;
using System;

namespace Khepri.Navigation
{
    /// <summary> The digital agent used to control a unit. </summary>
    public partial class AIController : Node
    {
        /// <summary> The unit this controller will control. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _unit;

        /// <summary> A reference to the player's controller. </summary>
        private PlayerController _playerController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            KnownEntity? targetEntity = _unit.Sensors.FindEntity(_playerController.PlayerUnit);
            if (targetEntity != null)
            {
                _unit.NavigationAgent.TargetPosition = targetEntity.LastKnownPosition;
                Vector3 nextPosition = _unit.NavigationAgent.GetNextPathPosition();
                Vector3 direction = _unit.GlobalPosition.DirectionTo(nextPosition).Normalized() * 0.5f;
                _unit.HandleInput(new MoveInput(direction, MoveType.WALKING));
            }
        }
    }
}
