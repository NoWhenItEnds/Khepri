using System;
using Godot;
using Jaypen.Utilities.Singletons;
using Khepri.UI.World;

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
        [ExportSubgroup("Windows")]
        [Export] private WorldWindow _worldWindow = null!;


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            _worldWindow.ForceUpdate();
        }
    }
}
