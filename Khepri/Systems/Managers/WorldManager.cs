using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Data.Entities;
using Khepri.Nodes;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all the nodes within the player's current view. Decoupled from the entity data objects. </summary>
    public partial class WorldManager : SingletonNode2D<WorldManager>
    {
        /// <summary> The camera node the player view the world through. </summary>
        [ExportGroup("Nodes")]
        [Export] private PlayerCamera _playerCamera = null!;


        /// <summary> The initial number of entity nodes in the pool, and the size it will try to shrink to. </summary>
        /// <remarks> This isn't the max size, as the pool will grow if necessary, just what it will try to hover around. </remarks>
        [ExportGroup("Settings")]
        [Export] private Int32 _idealEntityPoolSize = 100;


        /// <summary> The prefab used for spawning entity nodes. </summary>
        [ExportGroup("Resources")]
        [Export] private PackedScene _entityNodePrefab = null!;


        /// <summary> The entity nodes currently assigned to an in-view entity, keyed by the entity they represent. </summary>
        private Dictionary<Entity, EntityNode> _activeNodes = new Dictionary<Entity, EntityNode>();

        /// <summary> The entity nodes that are cleaned up and waiting in the pool for assignment. </summary>
        private Stack<EntityNode> _freeNodes = new Stack<EntityNode>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<WorldManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            // Pre-seed the pool with the ideal number of free nodes.
            for (Int32 i = 0; i < _idealEntityPoolSize; i++)
            {
                _freeNodes.Push(AddEntityNode());
            }
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            Rect2 viewRect = _playerCamera.GetViewRect();
            ReleaseHiddenNodes(viewRect);
            AssignVisibleNodes(viewRect);
        }


        /// <summary> Return any node whose entity has left the camera's view back to the free pool, leaving still-visible nodes untouched. </summary>
        /// <param name="viewRect"> The rectangle of the world currently visible through the camera. </param>
        private void ReleaseHiddenNodes(Rect2 viewRect)
        {
            List<Entity> hidden = new List<Entity>();

            foreach (KeyValuePair<Entity, EntityNode> pair in _activeNodes)
            {
                if (!IsNodeInView(viewRect, pair.Key, pair.Value))
                {
                    hidden.Add(pair.Key);
                }
            }

            foreach (Entity entity in hidden)
            {
                EntityNode node = _activeNodes[entity];
                node.Cleanup();

                _freeNodes.Push(node);
                _activeNodes.Remove(entity);
            }
        }


        /// <summary> Assign a free node to every in-view entity that does not already have one, leaving already-represented entities untouched. </summary>
        /// <param name="viewRect"> The rectangle of the world currently visible through the camera. </param>
        private void AssignVisibleNodes(Rect2 viewRect)
        {
            IEnumerable<Entity> entities = EntityManager.Instance?.GetEntities() ?? Array.Empty<Entity>();

            foreach (Entity entity in entities)
            {
                // No node exists yet to measure, so spawn once the entity's position enters view; its size is honoured from then on when releasing.
                if (viewRect.HasPoint(entity.GlobalPosition) && !_activeNodes.ContainsKey(entity))
                {
                    EntityNode node = GetFreeNode();
                    node.Build(entity);

                    _activeNodes.Add(entity, node);
                }
            }
        }


        /// <summary> Determine whether an entity's node still overlaps the view rectangle, using the node's own sprite-derived radius so large sprites stay rendered while partially visible. </summary>
        /// <param name="viewRect"> The rectangle of the world currently visible through the camera. </param>
        /// <param name="entity"> The entity whose position anchors the bounds. </param>
        /// <param name="node"> The node representing the entity, whose radius sizes the bounds. </param>
        /// <returns> True when any part of the node's bounds falls within the view. </returns>
        private static Boolean IsNodeInView(Rect2 viewRect, Entity entity, EntityNode node)
        {
            Vector2 extents = Vector2.One * node.Radius;
            Rect2 bounds = new Rect2(entity.GlobalPosition - extents, extents * 2f);

            return viewRect.Intersects(bounds);
        }


        /// <summary> Take a node from the free pool, growing the pool with a fresh node when none are currently available. </summary>
        /// <returns> A cleaned-up node that is ready to be assigned to an entity. </returns>
        private EntityNode GetFreeNode()
        {
            EntityNode node;

            if (_freeNodes.Count > 0)
            {
                node = _freeNodes.Pop();
            }
            else
            {
                node = AddEntityNode();
            }

            return node;
        }


        /// <summary> Create and correctly instantiate a new empty entity node, cleaned up and ready for assignment. </summary>
        /// <returns> The newly created node. </returns>
        private EntityNode AddEntityNode()
        {
            EntityNode node = _entityNodePrefab.Instantiate<EntityNode>();
            AddChild(node);
            node.Cleanup();

            return node;
        }
    }
}
