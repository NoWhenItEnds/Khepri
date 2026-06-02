using Godot;

namespace Khepri.UI.World
{
    /// <summary> Window displaying a description about the world, along with the player's means to interact with said world. </summary>
    public partial class TextWindow : Control
    {
        /// <summary> The label to show a description of the current situation. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _situationLabel = null!;


        /// <summary> Force the text window to reflect the current game's state. </summary>
        public void ForceUpdate()
        {
            _situationLabel.Text = "PLACEHOLDER";
        }
    }
}
