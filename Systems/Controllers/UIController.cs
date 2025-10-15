using System;
using Godot;
using Khepri.Entities.Devices;
using Khepri.Nodes.Singletons;
using Khepri.UI.HUD;
using Khepri.UI.HUD.Interaction;
using Khepri.UI.Windows;

namespace Khepri.Controllers
{
    /// <summary> The main controller for the game world's UI. </summary>
    public partial class UIController : SingletonControl<UIController>
    {
        /// <summary> A reference to the astrolabe in the HUD. </summary>
        [ExportGroup("Nodes")]
        [ExportSubgroup("HUD")]
        [Export] private Astrolabe _astrolabe;

        /// <summary> A reference to the player status bars in the HUD. </summary>
        [Export] private StatusBars _statusBars;

        /// <summary> A reference to the menu that displays interactable elements. </summary>
        [Export] private InteractionMenu _interactionMenu;


        /// <summary> A reference to the inventory window element. </summary>
        [ExportSubgroup("Windows")]
        [Export] private InventoryWindow _inventoryWindow;

        /// <summary> A window used to show the telescope's view. </summary>
        [Export] private TelescopeWindow _telescopeWindow;


        /// <summary> Whether a UI window is open. When this is true, the player shouldn't be able to control their character. </summary>
        public Boolean IsWindowOpen { get; private set; } = false;


        /// <summary> A reference to the player controller. </summary>
        private PlayerController _playerController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;

            ShowWindow(WindowType.NONE);
        }


        /// <summary> Tell the UI to show a particular window. </summary>
        /// <param name="window"> The window to show. A NONE indicates that all should be closed. </param>
        public void ShowWindow(WindowType window)
        {
            switch (window)
            {
                case WindowType.INVENTORY:
                    ToggleHUD(false);
                    _inventoryWindow.Visible = true;
                    _inventoryWindow.Initialise(_playerController.PlayerBeing.Inventory);
                    break;
                default:
                    ToggleHUD(true);
                    _inventoryWindow.Visible = false;
                    _telescopeWindow.Visible = false;
                    break;
            }
        }


        /// <summary> Show the telescope window to represent the view of a telescope. </summary>
        /// <param name="telescope"> A reference to the triggering telescope. </param>
        public void ShowTelescope(Telescope telescope)
        {
            ToggleHUD(false);
            _telescopeWindow.Visible = true;
            _telescopeWindow.Initialise(telescope);
        }


        /// <summary> Toggles whether the HUD is currently active. </summary>
        /// <param name="isActive"> The state to set HUD elements. </param>
        private void ToggleHUD(Boolean isActive)
        {
            IsWindowOpen = !isActive;
            _astrolabe.Visible = isActive;
            _statusBars.Visible = isActive;
            _interactionMenu.Visible = isActive;
        }
    }


    /// <summary> The kind of UI window. </summary>
    public enum WindowType
    {
        NONE,
        CHARACTER,
        INVENTORY,
        TELESCOPE
    }
}
