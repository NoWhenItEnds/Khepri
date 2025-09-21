using Godot;
using Khepri.Entities;
using Khepri.Models.Input;
using Khepri.Nodes.Singletons;
using System;

namespace Khepri.Controllers
{
    /// <summary> Allows the player to control an entity and the game world. </summary>
    public partial class PlayerController : SingletonNode<PlayerController>
    {
        /// <summary> The unit currently controlled by the player. </summary>
        [ExportGroup("Nodes")]
        [Export] public Unit PlayerUnit { get; private set; }

        /// <summary> The game world's main camera the player views through. </summary>
        [Export] public Camera3D MainCamera { get; private set; }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // TODO - Move into it's own controller / manager?
            MainCamera.GlobalPosition = PlayerUnit.CameraPosition.GlobalPosition;

            Single horizontal = Input.GetAxis("action_move_left", "action_move_right");
            Single vertical = Input.GetAxis("action_move_up", "action_move_down");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).Normalized();

            MoveType movementType = MoveType.IDLE;
            if (direction != Vector3.Zero)
            {
                movementType = Input.IsActionPressed("action_move_sprint") ? MoveType.SPRINTING : MoveType.WALKING;
            }

            MoveInput moveInput = new MoveInput(direction, movementType);
            PlayerUnit.HandleInput(moveInput);
        }
    }
}
