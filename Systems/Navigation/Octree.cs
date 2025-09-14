using Godot;
using Khepri.Entities;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;

namespace Khepri.Navigation
{
    /// <summary> The base octree data structure. </summary>
    public class Octree
    {
        /// <summary> The initial octree node that serves as the root node. </summary>
        public Octant Root { get; private set; }

        /// <summary> The total bounds of the octree. </summary>
        public Aabb Bounds { get; private set; }


        /// <summary> A list of the smallest leaf nodes of the tree. </summary>
        private List<Octant> _emptyOctants = new List<Octant>();


        /// <summary> The base octree data structure. </summary>
        /// <param name="entities"> The array of entities to build an octree around. </param>
        /// <param name="minimumNodeSize"> The minimum size our octants can shrink. </param>
        public Octree(IEntity[] entities, Single minimumNodeSize)
        {
            CalculateBounds(entities);
            GenerateTree(entities, minimumNodeSize);
        }


        /// <summary> Calculate the octree's total bounds. This will incapsulate all the given entities. </summary>
        /// <param name="entities"> The array of entities to build an octree around. </param>
        private void CalculateBounds(IEntity[] entities)
        {
            foreach (IEntity entity in entities)
            {
                Bounds = Bounds.Merge(entity.CollisionShape.GetAabb());
            }

            //  Square the box to make is a perfect cube.
            Vector3 size = Vector3.One * Math.Max(Math.Max(Bounds.Size.X, Bounds.Size.Y), Bounds.Size.Z);
            Bounds = new Aabb(Bounds.GetCenter() - (size * 0.5f), size);
        }


        /// <summary> Generate the octree. </summary>
        /// <param name="entities"> The entities to build around. </param>
        /// <param name="minimumNodeSize"> The minimum size our octants can shrink. </param>
        private void GenerateTree(IEntity[] entities, Single minimumNodeSize)
        {
            Root = new Octant(Bounds, minimumNodeSize);

            foreach (IEntity entity in entities)
            {
                Root.Divide(entity);
            }
        }


        private void GetEmptyLeaves()
        {

        }
    }
}
