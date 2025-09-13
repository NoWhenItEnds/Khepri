using Godot;
using Khepri.Entities;
using Khepri.Models.Input;
using System;

namespace Khepri.Controllers
{
    /// <summary> Allows the player to control an entity and the game world. </summary>
    public partial class PlayerController : Node
    {
        /// <summary> The unit currently controlled by the player. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _playerUnit;

        /// <summary> The game world's main camera the player views through. </summary>
        [Export] private Camera3D _mainCamera;


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _mainCamera.GlobalPosition = _playerUnit.CameraPosition.GlobalPosition;


            Single horizontal = Input.GetAxis("action_move_left", "action_move_right");
            Single vertical = Input.GetAxis("action_move_up", "action_move_down");
            // TODO - Add flying controls for Y-axis.
            var moveInput = new MoveInput(new Vector3(horizontal, 0f, vertical), MoveType.WALKING);
            _playerUnit.HandleInput(moveInput);
        }
    }
}
