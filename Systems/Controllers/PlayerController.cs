using Godot;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Nodes;
using Khepri.Nodes.Singletons;
using Khepri.Types.Extensions;
using System;
using System.Linq;

namespace Khepri.Controllers
{
    /// <summary> Allows the player to control an entity and the game world. </summary>
    public partial class PlayerController : SingletonNode<PlayerController>
    {
        /// <summary> The being representing the player's unit. </summary>
        [ExportGroup("Nodes")]
        [Export] public Being PlayerBeing { get; private set; }

        /// <summary> The current resource that the player is controlling. </summary>
        private IEntity _currentControlledEntity;


        /// <summary> Whether the user is currently using a controller. </summary>
        public Boolean IsUsingJoypad { get; private set; } = false;


        /// <summary> The index of the current interactable. </summary>
        private Int32 _currentInteractableIndex = 0;


        /// <summary> A reference to the window node. </summary>
        private Viewport _viewport;

        /// <summary> The game world's main camera the player views through. </summary>
        private WorldCamera _worldCamera;

        /// <summary> A reference to the game world's UI controller. </summary>
        private UIController _uiController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _viewport = GetViewport();
            _worldCamera = WorldCamera.Instance;
            _uiController = UIController.Instance;

            // Set up initial state.
            _currentControlledEntity = PlayerBeing;
            _worldCamera.SetTarget(PlayerBeing.CameraPosition);
            Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (!_uiController.IsWindowOpen || PlayerBeing != _currentControlledEntity)  // TODO - This feels like a hack to check it like this to allow the player to control a window.
            {
                Single moveHorizontal = Input.GetAxis("action_move_left", "action_move_right");
                Single moveVertical = Input.GetAxis("action_move_up", "action_move_down");
                Vector3 moveDirection = new Vector3(moveHorizontal, 0f, moveVertical).Normalized();

                MoveInput.MoveType movementType = MoveInput.MoveType.IDLE;
                if (moveDirection != Vector3.Zero)
                {
                    movementType = Input.IsActionPressed("action_move_sprint") ? MoveInput.MoveType.SPRINTING : MoveInput.MoveType.WALKING;
                }

                MoveInput moveInput = new MoveInput(moveDirection, movementType);
                _currentControlledEntity.HandleInput(moveInput);

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

            // Ensure that the interaction index is within range.
            _currentInteractableIndex = (Int32)MathExtensions.WrapValue(_currentInteractableIndex, PlayerBeing.UsableEntities.Count);
        }


        /// <inheritdoc/>
        public override void _UnhandledInput(InputEvent @event)
        {
            SetControllerType(@event);

            if (@event.IsActionReleased("action_toggle_ui"))
            {
                Input.MouseMode = !_uiController.IsWindowOpen ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.ConfinedHidden;
                _uiController.ShowWindow(!_uiController.IsWindowOpen ? WindowType.INVENTORY : WindowType.NONE);
                SetControllable(null);  // TODO - Is this the best place to reset the controller?
            }

            if (!_uiController.IsWindowOpen)
            {
                IEntity? currentInteractable = GetCurrentInteractable();
                if(currentInteractable != null)
                {
                    if (@event.IsActionReleased("action_examine"))
                    {
                        PlayerBeing.HandleInput(new ExamineInput(currentInteractable));
                    }
                    else if (@event.IsActionReleased("action_use"))
                    {
                        PlayerBeing.HandleInput(new UseInput(currentInteractable));
                    }
                    else if (@event.IsActionReleased("action_grab"))
                    {
                        PlayerBeing.HandleInput(new GrabInput(currentInteractable));
                    }
                }

                if (@event.IsActionReleased("action_ui_up"))
                {
                    _currentInteractableIndex = (Int32)MathExtensions.WrapValue(_currentInteractableIndex + 1, PlayerBeing.UsableEntities.Count);
                }
                else if (@event.IsActionReleased("action_ui_down"))
                {
                    _currentInteractableIndex = (Int32)MathExtensions.WrapValue(_currentInteractableIndex - 1, PlayerBeing.UsableEntities.Count);
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


        /// <summary> Set the current entity being controlled by the controller. </summary>
        /// <param name="controllable"> The new entity to control. A null resets it back to the default being. </param>
        public void SetControllable(IEntity? controllable = null)
        {
            _currentControlledEntity = controllable ?? PlayerBeing;
        }


        /// <summary> Get the currently selected entity the player is interacting with. </summary>
        /// <returns> Either a reference to the selected entity, or a null if there isn't one. </returns>
        public IEntity? GetCurrentInteractable() => PlayerBeing.UsableEntities.Count > 0 ? PlayerBeing.UsableEntities.ToArray()[_currentInteractableIndex] : null;
    }
}
