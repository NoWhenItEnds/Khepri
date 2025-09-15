using Godot;
using Khepri.Entities;
using Khepri.Nodes.Singletons;
using Khepri.Types.Extensions;
using System;

namespace Khepri.Navigation
{
    /// <summary> The world's navigation controller. </summary>
    /// <remarks> https://www.youtube.com/watch?v=gNmPmWR2vV4 && https://github.com/adammyhre/Unity-Octrees </remarks>
    public partial class NavigationController : SingletonNode<NavigationController>
    {
        /// <summary> A reference to the parent node of the game world's entities. </summary>
        [ExportGroup("Nodes")]
        [Export] private Node3D _entitiesParent;

        /// <summary> The minimum size of an octree node. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _minimumNodeSize = 1f;

        /// <summary> Shows the octree nodes. </summary>
        [ExportGroup("Debug")]
        [Export] private Boolean _showOctreeNodes = false;

        /// <summary> Shows the navigation waypoint. </summary>
        [Export] private Boolean _showNavigationWaypoints = false;


        /// <summary> A reference to the graph used to navigate the world. </summary>
        public readonly Graph NavigationGraph = new Graph();


        /// <summary> An array of the entities included within the octree. </summary>
        private IEntity[] _worldEntities = Array.Empty<IEntity>();

        /// <summary> A reference to the scene's current octree. </summary>
        private Octree? _octree = null;


        /// <inheritdoc/>
        public override void _Ready()
        {
            Update();
        }


        /// <summary> Updates the navigation graph. </summary>
        private void Update()
        {
            _worldEntities = _entitiesParent.GetChildrenOfType<IEntity>();
            _octree = new Octree(_worldEntities, _minimumNodeSize, NavigationGraph);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (_octree != null)
            {
                if (_showOctreeNodes) { _octree.Root.DrawNode(); }
                if (_showNavigationWaypoints) { NavigationGraph.DrawGraph(); }
            }
        }
    }
}
