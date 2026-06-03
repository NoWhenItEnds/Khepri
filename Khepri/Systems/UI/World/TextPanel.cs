using Godot;
using Khepri.Rooms;

namespace Khepri.UI.World
{
    /// <summary> Panel displaying a description about the world, along with the player's means to interact with said world. </summary>
    public partial class TextPanel : Control
    {
        /// <summary> The label to show a description of the currently selected room. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _roomLabel = null!;


        /// <summary> Force the text panel to display the current description / state of the given room. </summary>
        /// <param name="room"> The room to see the state of. </param>
        public void ForceUpdate(Room room)
        {
            _roomLabel.Text = room.BuildDescription();
        }
    }
}
