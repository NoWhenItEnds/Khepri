using Godot;

namespace Khepri.UI.World
{
    /// <summary> A window used to render characters, or display images of important objects that the player is examining. Also can render scenes of ongoing action. </summary>
    public partial class DisplayWindow : Control
    {
        /// <summary> The texture to show an image of the current situation. </summary>
        [ExportGroup("Nodes")]
        [Export] private TextureRect _situationTexture = null!;


        /// <summary> Force the display window to reflect the current game's state. </summary>
        public void ForceUpdate()
        {

        }
    }
}
