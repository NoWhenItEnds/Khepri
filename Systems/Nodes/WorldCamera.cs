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


        /// <summary> The modifier to allow the camera to move away from the followed node. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _cameraRadiusModifier = 4f;


        /// <summary> A reference to the window node. </summary>
        private Viewport _viewport;


        /// <summary> The node the camera is currently following. </summary>
        private Node3D? _followNode = null;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _viewport = GetViewport();
            _viewport.SizeChanged += SyncViewportSize;
            SetCameraSize(_mainCamera.Size);
            SyncViewportSize();
        }


        /// <summary> Set's the orthographic camera's size. </summary>
        /// <param name="size"> The distance the camera is zoomed. Higher values means that it is more zoomed out.</param>
        public void SetCameraSize(Single size = 12f)
        {
            _mainCamera.Size = size;
            _occlusionCamera.Size = size;
        }


        private void SyncViewportSize()
        {
            Vector2I viewportSize = (Vector2I)_viewport.GetVisibleRect().Size;
            _occlusionSubViewport.Size = viewportSize;
            _occlusionMaskSubViewport.Size = viewportSize;

            // Update the occlusion quad's size.
            Single aspectRatio = (Single)viewportSize.X / (Single)viewportSize.Y;
            Single height = _mainCamera.Size;
            Single width = height * aspectRatio;
            QuadMesh quad = _occlusionMesh.Mesh as QuadMesh;
            quad.Size = new Vector2(width, height);
        }


        /// <summary> Set the target node the camera follows. </summary>
        /// <param name="target"> A reference to the target node. </param>
        public void SetTarget(Node3D target) => _followNode = target;


        /// <summary> Updates the camera's position. </summary>
        /// <param name="relativeRatio"> A ratio to move the camera from the followed node. </param>
        public void UpdateCameraPosition(Vector2 relativeRatio)
        {
            if (_followNode != null)
            {
                GlobalPosition = _followNode.GlobalPosition + new Vector3(relativeRatio.X, 0f, relativeRatio.Y) * _cameraRadiusModifier;
            }
            _occlusionCamera.GlobalTransform = _mainCamera.GlobalTransform;
        }
    }
}
