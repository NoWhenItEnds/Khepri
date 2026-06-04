using Godot;
using Khepri.Entities;
using Khepri.Managers;
using Khepri.Rooms;

namespace Khepri.UI.Map
{
    /// <summary> The window the player uses to navigate the game world. </summary>
    public partial class MapWindow : Control
    {
        /// <summary> The panel that shows a map of the current room. </summary>
        [ExportGroup("Nodes")]
        [Export] private RoomPanel _roomPanel = null!;

        /// <summary> The panel that shows a map of the game world. </summary>
        [Export] private OverworldPanel _overworldPanel = null!;


        /// <summary> Forces both child panels to reflect the current game state by resolving the player and their containing room from the global managers. </summary>
        public void ForceUpdate()
        {
            Entity player = GameManager.Instance!.PlayerEntity;
            Room room     = RoomManager.Instance!.GetCurrentRoom(player);

            _roomPanel.ForceUpdate(room);
            _overworldPanel.ForceUpdate(room);
        }
    }
}
