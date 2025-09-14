using Godot;
using Khepri.Entities;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;

namespace Khepri.Nodes
{
    /// <summary> The navigation region for a given Z-level. </summary>
    public partial class OctreeGenerator : Node3D
    {
        /// <summary> The minimum size of an octree node. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _minimumNodeSize = 1f;

        private IEntity[] _worldEntities = Array.Empty<IEntity>();

        /// <summary> A reference to the scene's current octree. </summary>
        private Octree _octree;

        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldEntities = this.GetChildrenOfType<IEntity>();

            _octree = new Octree(_worldEntities, _minimumNodeSize);
        }

        public override void _Process(double delta)
        {
            DebugDraw3D.DrawBox(Vector3.Zero, Quaternion.Identity, Vector3.One, Colors.Red);
        }

    }


    public class Octree
    {
        public Octant Root { get; private set; }

        public Aabb Bounds { get; private set; }



        public Octree(IEntity[] entities, Single minimumNodeSize)
        {
            foreach (IEntity entity in entities)
            {
                Bounds.Merge(entity.CollisionShape.GetAabb());
            }
        }
    }


    public class Octant
    {

    }
}
