using Godot;
using Khepri.Entities;
using Khepri.Models.Input;
using Khepri.Nodes;
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


        /// <summary> A reference to the window node. </summary>
        private Viewport _viewport;

        /// <summary> The game world's main camera the player views through. </summary>
        private WorldCamera _worldCamera;


        /// <summary> Whether the ui is currently open. </summary>
        private Boolean _isUIOpen = false;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _viewport = GetViewport();

            _worldCamera = WorldCamera.Instance;
            _worldCamera.SetTarget(PlayerUnit.CameraPosition);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (!_isUIOpen)
            {
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

                // Handle camera.
                Vector2 radius = (Vector2)_viewport.GetVisibleRect().Size * 0.5f;
                Vector2 centeredMousePosition = _viewport.GetMousePosition() - (Vector2)_viewport.GetVisibleRect().Size * 0.5f;
                Vector2 ratio = centeredMousePosition / radius;
                _worldCamera.UpdateCameraPosition(ratio);
            }
        }


        /// <inheritdoc/>
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionReleased("action_toggle_ui"))
            {
                _isUIOpen = !_isUIOpen;
                Input.MouseMode = _isUIOpen ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.ConfinedHidden;
            }
        }
    }
}
