using System;
using Godot;
using Khepri.Nodes.Singletons;

namespace Khepri.Nodes
{
    /// <summary> The world's main camera the unit will see through. </summary>
    public partial class WorldCamera : SingletonNode3D<WorldCamera>
    {
        /// <summary> The main camera used to render the world. </summary>
        [ExportGroup("Nodes")]
        [Export] private Camera3D _mainCamera;

        /// <summary> The viewport used to capture the occlusion camera. </summary>
        [Export] private SubViewport _occlusionSubViewport;

        /// <summary> The camera used to render occlusions. </summary>
        [Export] private Camera3D _occlusionCamera;

        /// <summary> The viewport used to capture the occlusion mask camera. </summary>
        [Export] private SubViewport _occlusionMaskSubViewport;

        /// <summary> A reference to the occlusion mesh. </summary>
        [Export] private MeshInstance3D _occlusionMesh;


        /// <summary> A reference to the window node. </summary>
        private Window _window;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _window = GetWindow();
            _window.SizeChanged += SyncViewportSize;
            SyncViewportSize();
        }



        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _occlusionCamera.GlobalTransform = _mainCamera.GlobalTransform;
        }


        private void SyncViewportSize()
        {
            Vector2I viewportSize = _window.Size;
            _occlusionSubViewport.Size = viewportSize;
            _occlusionMaskSubViewport.Size = viewportSize;

            // Update the occlusion quad's size.
            Single aspectRatio = (Single)viewportSize.X / (Single)viewportSize.Y;
            Single height = _mainCamera.Size;
            Single width = height * aspectRatio;
            QuadMesh quad = _occlusionMesh.Mesh as QuadMesh;
            quad.Size = new Vector2(width, height);
        }
    }
}
