using Godot;
using Khepri.Nodes.Singletons;
using Khepri.UI.HUD;
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


        /// <summary> A reference to the inventory window element. </summary>
        [ExportSubgroup("Windows")]
        [Export] private InventoryWindow _inventoryWindow;


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
                    _inventoryWindow.Visible = true;
                    _inventoryWindow.Initialise(_playerController.PlayerUnit.Inventory);
                    break;
                default:
                    _inventoryWindow.Visible = false;
                    break;
            }
        }
    }


    /// <summary> The kind of UI window. </summary>
    public enum WindowType
    {
        NONE,
        CHARACTER,
        INVENTORY
    }
}
