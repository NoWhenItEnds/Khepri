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


        /// <summary> Moves the player into the room they selected on the overworld map. </summary>
        /// <remarks> Routes the player's choice through the same <see cref="RoomManager.MoveEntity"/> primitive the AI uses; the move is a no-op when the chosen room is not directly connected to the player's current one. </remarks>
        /// <param name="destination"> The room the player clicked. </param>
        private void OnRoomSelected(Room destination)
        {
            Entity player = GameManager.Instance!.PlayerEntity;
            RoomManager.Instance!.MoveEntity(player, destination);
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
