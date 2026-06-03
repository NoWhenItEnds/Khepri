using Godot;
using Khepri.Entities;
using Khepri.Managers;
using Khepri.Rooms;

namespace Khepri.UI.World
{
    /// <summary> The window the player sees and interacts with the world through. </summary>
    public partial class WorldWindow : Control
    {
        /// <summary> The node that displays information about the current state of the world. </summary>
        /// <remarks> Information such as time, temperature, etc. Passive information that doesn't fit in the description of a room, but humans intuitively understand. </remarks>
        [ExportGroup("Nodes")]
        [Export] private StatusBar _statusBar = null!;

        /// <summary> Window displaying a description about the world, along with the player's means to interact with said world. </summary>
        [Export] private TextWindow _textWindow = null!;

        /// <summary> A window used to render characters, or display images of important objects that the player is examining. Also can render scenes of ongoing action. </summary>
        [Export] private DisplayWindow _displayWindow = null!;


        /// <summary> Force the window to update. </summary>
        public void ForceUpdate()
        {
            Entity player = GameManager.Instance!.PlayerEntity;
            Room room = RoomManager.Instance!.GetCurrentRoom(player);

            _statusBar.ForceUpdate();
            _textWindow.ForceUpdate(room);
            _displayWindow.ForceUpdate();   // TODO - How do we know which entity to display? Do we even have the display update here?
        }
    }
}
