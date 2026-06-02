using Godot;
using Jaypen.Utilities.Singletons;
using System;

namespace Khepri.Managers
{
    /// <summary> The singleton manager for the game world's UI elements. All UI elements should be controlled through this. </summary>
    public partial class UIManager : SingletonControl<UIManager>
    {
        /// <summary> The node that set's the background image behind all the UI elements. </summary>
        /// <remarks> Should update to reflect the environment / setting of the current scene. </remarks>
        [ExportGroup("Nodes")]
        [Export] private TextureRect _background = null!;

        /// <summary> The window the player sees and interacts with the world through. </summary>
        [Export] private Control _worldWindow = null!; // TODO - Make own class.

        /// <summary> The node that displays information about the current state of the world. </summary>
        /// <remarks> Information such as time, temperature, etc. Passive information that doesn't fit in the description of a room, but humans intuitively understand. </remarks>
        [ExportSubgroup("World")]   // TODO - Move to their own master class.
        [Export] private Control _statusBar = null!; // TODO - Make own class.

        /// <summary> Window displaying a description about the world, along with the player's means to interact with said world. </summary>
        [Export] private Control _textWindow = null!; // TODO - Make own class.

        /// <summary> A window used to render characters, or display images of important objects that the player is examining. Also can render scenes of ongoing action. </summary>
        [Export] private Control _displayWindow = null!; // TODO - Make own class.
    }
}
