using Godot;
using Khepri.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Types
{
    /// <summary> A caching system for pulling objects from a pool of initialised objects on command. </summary>
    /// <typeparam name="TNode"> The kind of node in the pool. </typeparam>
    /// <typeparam name="TResource"> The kind of date the node represents. </typeparam>
    public class ObjectPool<TNode, TResource> where TNode : Node, IPoolable<TResource> where TResource : EntityResource
    {
        /// <summary> How many item nodes the item pool should initially contain. </summary>
        private Int32 _initialPoolSize = 100;


        /// <summary> A pool that holds objects that are in the game world. A boolean shows if an object is currently in use. </summary>
        private Dictionary<TNode, Boolean> _objectPool = new Dictionary<TNode, Boolean>();


        /// <summary> The node to use as the spawned nodes' parent. </summary>
        private readonly Node PARENT_NODE;

        /// <summary> The prefab to spawn additional objects from. </summary>
        private readonly PackedScene PREFAB;


        public ObjectPool(Node parentNode, PackedScene objectPrefab)
        {
            PARENT_NODE = parentNode;
            PREFAB = objectPrefab;

            // Add any existing objects.
            var children = PARENT_NODE.GetChildren();
            foreach (Node child in children)
            {
                if (child is TNode node)
                {
                    _objectPool.Add(node, true);
                }
            }

            // Build the pool.
            for (Int32 i = 0; i < _initialPoolSize; i++)
            {
                BuildItem();
            }
        }


        /// <summary> Create a new object for the pool. </summary>
        /// <returns> The created node. </returns>
        /// <exception cref="ArgumentException"> If the given prefab cannot be created as type T. </exception>
        private TNode BuildItem()
        {
            TNode obj = PREFAB.InstantiateOrNull<TNode>();
            if (obj == null)
            {
                throw new ArgumentException($"The given prefab cannot be instantiated into type {typeof(TNode)}.");
            }

            PARENT_NODE.AddChild(obj);
            _objectPool.Add(obj, false);

            // Depending upon the base type, getting it out of the way may be difficult.
            switch (obj)
            {
                case Node2D node2D:
                    node2D.GlobalPosition = new Vector2(-10000f, -10000f);
                    node2D.Visible = false;
                    break;
                case Node3D node3D:
                    node3D.GlobalPosition = new Vector3(0f, -10000f, 0f);
                    node3D.Visible = false;
                    break;
                case Control control:
                    control.GlobalPosition = new Vector2(-100f, -100f);
                    control.Visible = false;
                    break;
            }

            return obj;
        }


        /// <summary> Get a new object by pulling from the pool. </summary>
        /// <returns> The now active object. </returns>
        public TNode GetAvailableObject()
        {
            TNode? result = null;

            // Attempt to find a free object in the pool.
            foreach (KeyValuePair<TNode, Boolean> obj in _objectPool)
            {
                if (!obj.Value)    // If the obj is free.
                {
                    result = obj.Key;
                    break;
                }
            }

            // If an item was not found, expand the pool.
            if (result == null)
            {
                result = BuildItem();
            }

            _objectPool[result] = true;

            // Make the pulled object visible.
            switch (result)
            {
                case Node2D node2D:
                    node2D.Visible = true;
                    break;
                case Node3D node3D:
                    node3D.Visible = true;
                    break;
                case Control control:
                    control.Visible = true;
                    break;
            }

            return result;
        }


        /// <summary> Free all the currently in use items. </summary>
        public void FreeAll()
        {
            TNode[] activeObjects = GetActiveObjects();
            foreach (TNode obj in activeObjects)
            {
                FreeObject(obj);
            }
        }


        /// <summary> Free an object and return it to the pool. </summary>
        /// <param name="obj"> The object to return. </param>
        public void FreeObject(TNode obj)
        {
            _objectPool[obj] = false;

            // Depending upon the base type, getting it out of the way may be difficult.
            switch (obj)
            {
                case Node2D node2D:
                    node2D.GlobalPosition = new Vector2(-10000f, -10000f);
                    node2D.Visible = false;
                    break;
                case Node3D node3D:
                    node3D.GlobalPosition = new Vector3(0f, -10000f, 0f);
                    node3D.Visible = false;
                    break;
                case Control control:
                    control.GlobalPosition = new Vector2(-100f, -100f);
                    control.Visible = false;
                    break;
            }
        }


        /// <summary> Get an array of all the currently active objects. </summary>
        /// <returns> All the objects that are currently in use. </returns>
        public TNode[] GetActiveObjects() => _objectPool.Where(x => x.Value).Select(x => x.Key).ToArray();


        /// <summary> Get an array of all the pool's objects. </summary>
        /// <returns> An array of all the objects in the pool. </returns>
        public TNode[] GetAllObjects() => _objectPool.Select(x => x.Key).ToArray();
    }


    /// <summary> Represents an item as stored in a pool. </summary>
    public interface IPoolable<T> where T : EntityResource
    {
        /// <summary> The data component representing the entity's data. </summary>
        public T Resource { get; protected set; }


        /// <summary> Initialise the object by setting its internal resource. </summary>
        /// <param name="resource"> The resource the object represents. </param>
        public void Initialise(T resource)
        {
            Resource = resource;
        }


        /// <summary> Free the object back to the object pool. </summary>
        public void FreeObject();
    }
}
