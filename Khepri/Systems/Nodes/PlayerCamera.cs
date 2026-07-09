using System;
using Godot;
using Khepri.Controllers;
using Khepri.Data.Entities;

namespace Khepri.Nodes
{
    /// <summary> The camera node the player view the world through. </summary>
    public partial class PlayerCamera : Node2D
    {
        /// <summary> The game world's main camera node. </summary>
        [ExportGroup("Nodes")]
        [Export] private Camera2D _mainCamera = null!;


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            // Track the player's ship so the view travels with it. The world manager culls against the
            // camera's own centre, so moving the camera here is all that's needed for culling to follow too.
            Ship? ship = PlayerController.Instance?.PlayerShip;

            if (ship != null)
            {
                GlobalPosition = ship.GlobalPosition;
            }
        }


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
