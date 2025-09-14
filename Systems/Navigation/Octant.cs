using Godot;
using Khepri.Entities;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;

namespace Khepri.Navigation
{
    /// <summary> An octree node. Recursively represents a voxelised unit of space. </summary>
    public class Octant
    {
        /// <summary> The internal id of the next octant generated. </summary>
        private static Int32 _nextId;

        /// <summary> The unique identifier of this octant. </summary>
        public readonly Int32 Id;

        /// <summary> The boundary of this octant. </summary>
        public Aabb Bounds;

        /// <summary> A list of all the entities either within or partially within the bounds of the octant. </summary>
        public readonly List<IEntity> BoundEntities = new List<IEntity>();

        /// <summary> This octant's children. </summary>
        public Octant[] Children = Array.Empty<Octant>();

        /// <summary> Whether the current octant is a leaf, aka, it has no children. </summary>
        public Boolean IsLeaf => Children.Length == 0;


        /// <summary> This octant's children boundaries. </summary>
        private Aabb[] _childBounds = new Aabb[8];

        /// <summary> The minimum size the node can be. </summary>
        private Single _minimumNodeSize;


        /// <summary> An octree node. Recursively represents a voxelised unit of space. </summary>
        /// <param name="bounds"> The boundary of this octant. </param>
        /// <param name="minimumNodeSize"> The minimum size our octants can shrink. </param>
        public Octant(Aabb bounds, Single minimumNodeSize)
        {
            Id = _nextId++;
            Bounds = bounds;
            _minimumNodeSize = minimumNodeSize;

            Vector3 childrenSize = bounds.Size * 0.5f;
            Vector3 childrenOffset = bounds.Size * 0.25f;

            // Divide the space into 8 quadrants using some fancy bit-math.
            for (Int32 i = 0; i < 8; i++)
            {
                Vector3 childCenter = bounds.GetCenter();
                childCenter = new Vector3(
                    childCenter.X + childrenOffset.X * ((i & 1) == 0 ? -1 : 1),
                    childCenter.Y + childrenOffset.Y * ((i & 2) == 0 ? -1 : 1),
                    childCenter.Z + childrenOffset.Z * ((i & 4) == 0 ? -1 : 1)
                );
                _childBounds[i] = new Aabb(childCenter - (childrenSize * 0.5f), childrenSize);  // The position of an Aabb isn't its center, but its top-left corner.
            }
        }


        /// <summary> Subdivide the octant further around the given entity. </summary>
        /// <param name="entity"> The entity to subdivide around. </param>
        public void Divide(IEntity entity)
        {
            Aabb currentAabb = entity.CollisionShape.GetAabb();
            if (Bounds.Size.X <= _minimumNodeSize)
            {
                BoundEntities.Add(entity);
                return;
            }

            // If the array isn't initialised yet, do so.
            if (Children.Length == 0) { Children = new Octant[8]; }

            // We only want to divide if there is not an entity's edge in the division.
            Boolean intersectedChild = false;
            for (Int32 i = 0; i < 8; i++)
            {
                Children[i] ??= new Octant(_childBounds[i], _minimumNodeSize);  // Initialise the children.

                // An intersection means we need to keep dividing.
                if (currentAabb.Intersects(_childBounds[i]))
                {
                    Children[i].Divide(entity);
                    intersectedChild = true;
                }
            }

            if (!intersectedChild)
            {
                BoundEntities.Add(entity);
            }
        }


        /// <summary> Draw a debug representation of the octant node. </summary>
        public void DrawNode()
        {
            DebugDraw3D.DrawAabb(Bounds, Colors.Green);

            // Draw highlight the intersecting object.
            foreach (IEntity entity in BoundEntities)
            {
                Aabb currentAabb = entity.CollisionShape.GetAabb();
                if (currentAabb.Intersects(Bounds))
                {
                    DebugDraw3D.DrawAabb(currentAabb, Colors.Red);
                }
            }

            // Recursively draw its children octants.
            if (Children.Length != 0)
            {
                foreach (Octant child in Children)
                {
                    if (child != null)
                    {
                        child.DrawNode();
                    }
                }
            }
        }
    }
}
