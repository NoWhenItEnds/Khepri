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


        /// <summary> Forces both child panels to reflect the current game state by resolving the player and their containing room from the global managers. </summary>
        public void ForceUpdate()
        {
            Entity player = GameManager.Instance!.PlayerEntity;
            Room? room    = RoomManager.Instance!.GetCurrentRoom(player);

            // A null room means the player is in none (already logged); nothing to render this frame.
            if (room is not null)
            {
                _roomPanel.ForceUpdate(room);
                _overworldPanel.ForceUpdate(room);
            }
        }
    }
}
