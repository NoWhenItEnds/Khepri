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

        /// <summary> The graph this octree will use for navigation. </summary>
        public readonly Graph NavigationGraph;


        /// <summary> A list of the smallest leaf nodes of the tree. </summary>
        private List<Octant> _emptyOctants = new List<Octant>();


        /// <summary> The base octree data structure. </summary>
        /// <param name="entities"> The array of entities to build an octree around. </param>
        /// <param name="minimumNodeSize"> The minimum size our octants can shrink. </param>
        /// <param name="navigationGraph"> The graph this octree will use for navigation. </param>
        public Octree(IEntity[] entities, Single minimumNodeSize, Graph navigationGraph)
        {
            NavigationGraph = navigationGraph;

            CalculateBounds(entities);
            GenerateTree(entities, minimumNodeSize);

            GetEmptyLeaves(Root);
            GetEdges();
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


        /// <summary> Gets all the empty leaves in the given octant. </summary>
        /// <param name="octant"> The octant to search for empty leaves. </param>
        private void GetEmptyLeaves(Octant octant)
        {
            if (octant.IsLeaf && octant.BoundEntities.Count == 0)
            {
                _emptyOctants.Add(octant);
                NavigationGraph.AddNode(octant);
                return;
            }

            // Guard clause to ensure we don't step through empty children.
            if (octant.Children.Length == 0) { return; }

            // Recurse for all the children.
            foreach (Octant child in octant.Children)
            {
                GetEmptyLeaves(child);
            }

            // Add edges.
            for (Int32 i = 0; i < octant.Children.Length; i++)
            {
                for (Int32 j = i + 1; j < octant.Children.Length; j++)
                {
                    if (i != j)
                    {
                        NavigationGraph.AddEdge(octant.Children[i], octant.Children[j]);
                    }
                }
            }
        }


        /// <summary> Gets and builds the octree's edges by looking at the leaf nodes. </summary>
        private void GetEdges()
        {
            foreach (Octant leaf in _emptyOctants)
            {
                foreach (Octant otherLeaf in _emptyOctants)
                {
                    Aabb current = leaf.Bounds;
                    Aabb other = otherLeaf.Bounds;
                    if (current.Grow(current.Size.X * 0.1f).Intersects(other.Grow(other.Size.X * 0.1f)))
                    {
                        NavigationGraph.AddEdge(leaf, otherLeaf);
                    }
                }
            }
        }
    }
}
