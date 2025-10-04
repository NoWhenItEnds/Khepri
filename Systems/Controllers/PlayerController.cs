using Godot;
using Khepri.Entities.Actors;
using Khepri.Models.Input;
using Khepri.Nodes;
using Khepri.Nodes.Singletons;
using System;
using System.Linq;

namespace Khepri.Controllers
{
    /// <summary> Allows the player to control an entity and the game world. </summary>
    public partial class PlayerController : SingletonNode<PlayerController>
    {
        /// <summary> The unit currently controlled by the player. </summary>
        [ExportGroup("Nodes")]
        [Export] public Unit PlayerUnit { get; private set; }


        /// <summary> Emitted when the player's interaction selection changed. </summary>
        public event Action<Int32> SelectionChanged;


        /// <summary> Whether the user is currently using a controller. </summary>
        public Boolean IsUsingJoypad { get; private set; } = false;


        /// <summary> A reference to the window node. </summary>
        private Viewport _viewport;

        /// <summary> The game world's main camera the player views through. </summary>
        private WorldCamera _worldCamera;

        /// <summary> A reference to the game world's UI controller. </summary>
        private UIController _uiController;


        /// <summary> Whether the ui is currently open. </summary>
        private Boolean _isUIOpen = false;

        /// <summary> The index of the currently selected item on the UI. </summary>
        private Int32 _currentSelection = 0;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _viewport = GetViewport();
            _worldCamera = WorldCamera.Instance;
            _uiController = UIController.Instance;

            // Set up initial state.
            _worldCamera.SetTarget(PlayerUnit.CameraPosition);
            Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (!_isUIOpen)
            {
                Single moveHorizontal = Input.GetAxis("action_move_left", "action_move_right");
                Single moveVertical = Input.GetAxis("action_move_up", "action_move_down");
                Vector3 moveDirection = new Vector3(moveHorizontal, 0f, moveVertical).Normalized();

                MoveType movementType = MoveType.IDLE;
                if (moveDirection != Vector3.Zero)
                {
                    movementType = Input.IsActionPressed("action_move_sprint") ? MoveType.SPRINTING : MoveType.WALKING;
                }

                MoveInput moveInput = new MoveInput(moveDirection, movementType);
                PlayerUnit.HandleInput(moveInput);

                // Handle camera.
                Vector2 ratio = Vector2.Zero;
                if (IsUsingJoypad)
                {
                    Single cameraHorizontal = Input.GetAxis("action_camera_left", "action_camera_right");
                    Single cameraVertical = Input.GetAxis("action_camera_up", "action_camera_down");
                    ratio = new Vector2(cameraHorizontal, cameraVertical);
                }
                else
                {
                    Vector2 radius = (Vector2)_viewport.GetVisibleRect().Size * 0.5f;
                    Vector2 centeredMousePosition = _viewport.GetMousePosition() - (Vector2)_viewport.GetVisibleRect().Size * 0.5f;
                    ratio = centeredMousePosition / radius;
                }
                _worldCamera.UpdateCameraPosition(ratio);
            }
        }


        /// <inheritdoc/>
        public override void _UnhandledInput(InputEvent @event)
        {
            SetControllerType(@event);

            if (@event.IsActionReleased("action_toggle_ui"))
            {
                _isUIOpen = !_isUIOpen;
                Input.MouseMode = _isUIOpen ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.ConfinedHidden;
                _uiController.ShowWindow(_isUIOpen ? WindowType.INVENTORY : WindowType.NONE);
            }

            if (!_isUIOpen)
            {
                if (PlayerUnit.UsableEntities.Count > 0)
                {
                    if (@event.IsActionReleased("action_use"))
                    {
                        PlayerUnit.HandleInput(new UseInput(PlayerUnit.UsableEntities.ToArray()[_currentSelection]));
                        _currentSelection = Math.Clamp(_currentSelection, 0, PlayerUnit.UsableEntities.Count - 1);
                    }
                    else if (@event.IsActionReleased("action_ui_up"))
                    {
                        _currentSelection = Math.Clamp(_currentSelection + 1, 0, PlayerUnit.UsableEntities.Count - 1);
                        SelectionChanged?.Invoke(_currentSelection);
                    }
                    else if (@event.IsActionReleased("action_ui_down"))
                    {
                        _currentSelection = Math.Clamp(_currentSelection - 1, 0, PlayerUnit.UsableEntities.Count - 1);
                        SelectionChanged?.Invoke(_currentSelection);
                    }
                }
            }
        }


        /// <summary> Check and set whether the player is using a controller or not. </summary>
        /// <param name="event"> The event to check. </param>
        private void SetControllerType(InputEvent @event)
        {
            switch (@event)
            {
                case InputEventKey or InputEventMouse:
                    IsUsingJoypad = false;
                    break;
                case InputEventJoypadButton:
                case InputEventJoypadMotion { AxisValue: < -0.1f or > 0.1f }:
                    IsUsingJoypad = true;
                    break;
            }
        }
    }
}
