using Godot;
using Khepri.Entities;
using Khepri.Managers;
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


        /// <inheritdoc/>
        public override void _Ready()
        {
            _overworldPanel.RoomSelected += OnRoomSelected;
        }


        /// <summary> Hands the room the player selected to their controller as a move request. </summary>
        /// <remarks> The controller turns the click into an action the <see cref="TurnManager"/> performs on the player's next turn, rather than the UI mutating the world directly; selecting a room that is not directly connected yields a move that simply fails when performed, leaving the player free to choose again. </remarks>
        /// <param name="destination"> The room the player clicked. </param>
        private void OnRoomSelected(Room destination)
        {
            TurnManager.Instance!.Player.Select(destination);
        }


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
