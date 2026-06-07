using Godot;
using Khepri.Entities;
using Khepri.Rooms;

namespace Khepri.UI.World.Tabs
{
    /// <summary> The panel holding optional tabs the user can click between. </summary>
    public partial class TabPanel : Control
    {
        /// <summary> A panel used to render characters, or display images of important objects that the player is examining. Also can render scenes of ongoing action. </summary>
        [ExportGroup("Nodes")]
        [Export] private DisplayPanel _displayPanel = null!;

        /// <summary> The panel that shows a map of the current room. </summary>
        [Export] private LocalPanel _localPanel = null!;

        /// <summary> The panel that shows a map of the game world. </summary>
        [Export] private OverworldPanel _overworldPanel = null!;

        /// <summary> Force the UI element to reflect the current game's state. </summary>
        /// <param name="playerEntity"> The currently controlled player entity. </param>
        /// <param name="playerRoom"> The room the player currently inhabits. </param>
        public void ForceUpdate(Entity playerEntity, Room playerRoom)
        {
            //_displayPanel.ForceUpdate(room);  // TODO - Not sure what to do here.
            _localPanel.ForceUpdate(playerRoom);
            _overworldPanel.ForceUpdate(playerRoom);
        }
    }
}
