using System;
using Godot;
using Khepri.Controllers;
using Khepri.Managers;
using Khepri.World.Entities;

namespace Khepri.Nodes
{
    /// <summary> The camera node the player view the world through. </summary>
    public partial class PlayerCamera : Node2D
    {
        /// <summary> The game world's main camera node. </summary>
        [ExportGroup("Nodes")]
        [Export] private Camera2D _mainCamera = null!;


        /// <summary> A reference to the player's controller. </summary>
        private PlayerController _playerController = null!;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = ControllerManager.Instance?.PlayerController ?? throw new ArgumentNullException("Unable to find ControllerManager.");
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            // Track the player's ship so the view travels with it. The world manager culls against the
            // camera's own centre, so moving the camera here is all that's needed for culling to follow too.
            Entity? target = _playerController.Entity; // TODO - Shouldn't know about the player, have a controller mouse to follow.

            if (target != null)
            {
                GlobalPosition = target.Position;
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


        /// <summary> Determine whether any part of an entity overlaps the view rectangle. </summary>
        /// <param name="entity"> The entity whose position and radius size the bounds. </param>
        /// <returns> True when any part of the entity's bounds falls within the view. </returns>
        public Boolean IsEntityInView(Entity entity)
        {
            Vector2 extents = Vector2.One * entity.Radius;
            Rect2 bounds = new Rect2(entity.Position - extents, extents * 2f);

            return GetViewRect().Intersects(bounds);
        }
    }
}
