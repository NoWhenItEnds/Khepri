using Godot;

namespace Khepri.UI.Map
{
    /// <summary> The window the player uses to navigate the game world. </summary>
    public partial class MapWindow : Control
    {
        /// <summary> The panel that shows a map of the current room. </summary>
        [ExportGroup("Nodes")]
        [Export] private Control _roomPanel = null!;        // TODO - Make own class.

        /// <summary> The panel that shows a map of the game world. </summary>
        [Export] private Control _overworldPanel = null!;   // TODO - Make own class.
    }
}
