using Godot;
using Khepri.Controllers;
using Khepri.Entities;
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
            _unit.NavigationAgent.TargetPosition = _playerController.PlayerUnit.GlobalPosition;
            Vector3 nextPosition = _unit.NavigationAgent.GetNextPathPosition();
            Vector3 direction = (nextPosition - _unit.GlobalPosition).Normalized();
            _unit.HandleInput(new MoveInput(direction, MoveType.WALKING));
        }
    }
}
