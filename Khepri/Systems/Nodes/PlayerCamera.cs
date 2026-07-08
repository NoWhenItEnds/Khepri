using Godot;

namespace Khepri.Nodes
{
    /// <summary> The camera node the player view the world through. </summary>
    public partial class PlayerCamera : Node2D
    {
        /// <summary> The game world's main camera node. </summary>
        [ExportGroup("Nodes")]
        [Export] private Camera2D _mainCamera = null!;


        /// <summary> Get the rectangle of the game world that is currently visible through the camera, expressed in global coordinates. </summary>
        /// <returns> The visible world-space rectangle. </returns>
        public Rect2 GetViewRect()
        {
            Vector2 size = GetViewportRect().Size / _mainCamera.Zoom;
            Vector2 center = _mainCamera.GetScreenCenterPosition();

            return new Rect2(center - size / 2f, size);
        }
    }
}
