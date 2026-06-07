using Godot;
using Khepri.Entities;
using Khepri.Managers;
using Khepri.Rooms;
using Khepri.UI.World.Rooms;
using Khepri.UI.World.Tabs;

namespace Khepri.UI.World
{
    /// <summary> The window the player sees and interacts with the world through. </summary>
    public partial class WorldWindow : Control
    {
        /// <summary> The node that displays information about the current state of the world. </summary>
        /// <remarks> Information such as time, temperature, etc. Passive information that doesn't fit in the description of a room, but humans intuitively understand. </remarks>
        [ExportGroup("Nodes")]
        [Export] private StatusBar _statusBar = null!;

        /// <summary> The panel displaying a description about the current room the player inhabits. </summary>
        [Export] private RoomPanel _roomPanel = null!;

        /// <summary> The panel holding optional tabs the user can click between. </summary>
        [Export] private TabPanel _tabPanel = null!;


        /// <summary> Force the window to update. </summary>
        public void ForceUpdate()
        {
            Entity player = GameManager.Instance!.PlayerEntity;
            Room? room = RoomManager.Instance!.GetCurrentRoom(player);

            // A null room means the player is in none (already logged); nothing to render this frame.
            if (room != null)
            {
                _statusBar.ForceUpdate(player, room);
                _roomPanel.ForceUpdate(player, room);
                _tabPanel.ForceUpdate(player, room);
            }
        }
    }
}
