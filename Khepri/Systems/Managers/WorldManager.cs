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


        /// <summary> A pool of the entity node that currently exist in the game world, and a boolean representing if it's currently being used. </summary>
        private Dictionary<EntityNode, Boolean> _entityPool = new Dictionary<EntityNode, Boolean>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<WorldManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            // Build the entity pool.
            for (Int32 i = 0; i < _idealEntityPoolSize; i++)
            {
                AddEntityNode();
            }
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            base._Process(delta);
        }


        /// <summary> Create and correctly instantiate a new entity node, adding it to the node pool. </summary>
        /// <param name="entity"> The entity data object the node will represent. A null, the default, just adds an empty node to the pool. </param>
        private void AddEntityNode(Entity? entity = null)
        {
            EntityNode node = _entityNodePrefab.Instantiate<EntityNode>();
            AddChild(node);

            if (entity != null)
            {
                node.Build(entity);
            }
            else
            {
                node.Cleanup();
            }

            _entityPool.Add(node, entity != null);
        }
    }
}
