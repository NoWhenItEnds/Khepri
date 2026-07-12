using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Controllers;
using Khepri.Nodes;
using Khepri.World;
using Khepri.World.Entities;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all the nodes within the player's current view. Decoupled from the entity data objects. </summary>
    public partial class NodeManager : SingletonNode2D<NodeManager>
    {
        /// <summary> The camera node the player view the world through. </summary>
        [ExportGroup("Nodes")]
        [Export] private PlayerCamera _playerCamera = null!;

        /// <summary> The node to parent spawned entity nodes to. </summary>
        [Export] private Node2D _entityParent = null!;


        /// <summary> The initial number of entity nodes in the pool, and the size it will try to shrink to. </summary>
        /// <remarks> This isn't the max size, as the pool will grow if necessary, just what it will try to hover around. </remarks>
        [ExportGroup("Settings")]
        [Export] private Int32 _idealEntityPoolSize = 100;


        /// <summary> The prefab used for spawning entity nodes. </summary>
        [ExportGroup("Resources")]
        [Export] private PackedScene _entityNodePrefab = null!;


        /// <summary> A reference to the player's controller. </summary>
        private PlayerController _playerController = null!;

        /// <summary> The entity nodes currently assigned to an in-view entity, keyed by the entity they represent. </summary>
        private Dictionary<Entity, EntityNode> _activeNodes = new Dictionary<Entity, EntityNode>();

        /// <summary> The entity nodes that are cleaned up and waiting in the pool for assignment. </summary>
        private Stack<EntityNode> _freeNodes = new Stack<EntityNode>();

        /// <summary> Scratch list reused every frame when collecting the entities whose nodes should be released, saving a per-frame allocation. </summary>
        private readonly List<Entity> _hiddenBuffer = new List<Entity>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<NodeManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = ControllerManager.Instance?.PlayerController ?? throw new ArgumentNullException("Unable to find ControllerManager.");

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
            ShrinkPool();
        }


        /// <summary> Return any node whose entity has left the camera's view back to the free pool, leaving still-visible nodes untouched. </summary>
        /// <param name="viewRect"> The rectangle of the world currently visible through the camera. </param>
        private void ReleaseHiddenNodes(Rect2 viewRect)
        {
            SolarSystem? viewedSystem = _playerController.Entity?.System;

            _hiddenBuffer.Clear();

            foreach (KeyValuePair<Entity, EntityNode> pair in _activeNodes)
            {
                // An entity that is no longer part of the viewed system — a restored save replaced the
                // world, or a future jump moved the player elsewhere — releases its node wherever it
                // sits on screen, so stale worlds clean themselves up on the next cull.
                if (pair.Key.System != viewedSystem || !IsEntityInView(viewRect, pair.Key))
                {
                    _hiddenBuffer.Add(pair.Key);
                }
            }

            foreach (Entity entity in _hiddenBuffer)
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
            // The view shows the system the player's controlled entity is currently within; with no
            // player ship (or a ship lost in null-space) there is simply nothing to draw.
            IEnumerable<Entity> entities = _playerController.Entity?.System?.GetEntities() ?? Array.Empty<Entity>();

            foreach (Entity entity in entities)
            {
                if (IsEntityInView(viewRect, entity) && !_activeNodes.ContainsKey(entity))
                {
                    EntityNode node = GetFreeNode();
                    node.Build(entity);

                    _activeNodes.Add(entity, node);
                }
            }
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


        /// <summary> Shrink the pool back toward its ideal size, retiring at most one free node per frame so a burst of released nodes decays gently instead of freeing in a spike. </summary>
        private void ShrinkPool()
        {
            if (_freeNodes.Count > _idealEntityPoolSize)
            {
                EntityNode node = _freeNodes.Pop();
                node.QueueFree();
            }
        }


        /// <summary> Create and correctly instantiate a new empty entity node, cleaned up and ready for assignment. </summary>
        /// <returns> The newly created node. </returns>
        private EntityNode AddEntityNode()
        {
            EntityNode node = _entityNodePrefab.Instantiate<EntityNode>();
            _entityParent.AddChild(node);
            node.Cleanup();

            return node;
        }


        /// <summary> Whether any part of an entity's bounds overlaps the view rectangle, so its node should be kept assigned. </summary>
        /// <param name="viewRect"> The rectangle of the world currently visible through the camera. </param>
        /// <param name="entity"> The entity whose position and radius size the bounds. </param>
        /// <returns> True when any part of the entity's bounds falls within the view. </returns>
        private static Boolean IsEntityInView(Rect2 viewRect, Entity entity)
        {
            Vector2 extents = Vector2.One * entity.Radius;
            Rect2 bounds = new Rect2(entity.Position - extents, extents * 2f);

            return viewRect.Intersects(bounds);
        }
    }
}
