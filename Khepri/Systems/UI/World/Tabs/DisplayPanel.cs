using Godot;

namespace Khepri.UI.World.Tabs
{
    /// <summary> A panel used to render characters, or display images of important objects that the player is examining. Also can render scenes of ongoing action. </summary>
    public partial class DisplayPanel : Control
    {
        /// <summary> The texture to show an image of the currently selected entity. </summary>
        [ExportGroup("Nodes")]
        [Export] private TextureRect _entityTexture = null!;


        /// <summary> Force the display panel to reflect the current game's state. </summary>
        public void ForceUpdate()
        {

        }
    }
}
