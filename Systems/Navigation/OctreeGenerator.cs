using Godot;
using Khepri.Entities;
using Khepri.Types.Extensions;
using System;

namespace Khepri.Navigation
{
    /// <summary> The navigation region for a given Z-level. </summary>
    public partial class OctreeGenerator : Node3D
    {
        /// <summary> The minimum size of an octree node. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _minimumNodeSize = 1f;

        /// <summary> Activate the debugging helpers. </summary>
        [Export] private Boolean _isDebug = false;

        public readonly Graph NavigationWaypoints = new Graph();

        private IEntity[] _worldEntities = Array.Empty<IEntity>();

        /// <summary> A reference to the scene's current octree. </summary>
        private Octree _octree;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldEntities = this.GetChildrenOfType<IEntity>();
            _octree = new Octree(_worldEntities, _minimumNodeSize, NavigationWaypoints);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (_isDebug)
            {
                _octree.Root.DrawNode();
                NavigationWaypoints.DrawGraph();
            }
        }
    }
}
