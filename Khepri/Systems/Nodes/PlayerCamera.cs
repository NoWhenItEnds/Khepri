using Godot;

namespace Khepri.Nodes
{
    /// <summary> The camera node the player view the world through. </summary>
    public partial class PlayerCamera : Node2D
    {
        /// <summary> The game world's main camera node. </summary>
        [ExportGroup("Nodes")]
        [Export] private Camera2D _mainCamera = null!;
    }
}
