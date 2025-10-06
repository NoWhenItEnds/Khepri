using Godot;
using System;
using System.Collections.Generic;

namespace Khepri.Types
{
    /// <summary> A caching system for pulling objects from a pool of initialised objects on command. </summary>
    /// <typeparam name="T"> The kind of node in the pool. </typeparam>
    public class ObjectPool<T> where T : Node
    {
        /// <summary> How many item nodes the item pool should contain. </summary>
        private Int32 _poolSize = 100;


        /// <summary> A pool that holds objects that are in the game world. A boolean shows if an object is currently in use. </summary>
        private Dictionary<T, Boolean> _objectPool = new Dictionary<T, Boolean>();


        /// <summary> The node to use as the spawned nodes' parent. </summary>
        private readonly Node PARENT_NODE;

        /// <summary> The prefab to spawn additional objects from. </summary>
        private readonly PackedScene PREFAB;


        public ObjectPool(Node parentNode, PackedScene objectPrefab)
        {
            PARENT_NODE = parentNode;
            PREFAB = objectPrefab;

            // First look for any existing items.
            var existingNodes = parentNode.GetChildren();
            foreach (Node node in existingNodes)
            {
                if (node is T obj)
                {
                    _objectPool.Add(obj, true);
                }
            }

            // Build the pool.
            Int32 initialLength = existingNodes.Count;
            for (Int32 i = 0; i < _poolSize - initialLength; i++)
            {
                T obj = BuildItem();
                _objectPool.Add(obj, false);
            }
        }


        /// <summary> Create a new object for the pool. </summary>
        /// <returns> The created node. </returns>
        /// <exception cref="ArgumentException"> If the given prefab cannot be created as type T. </exception>
        private T BuildItem()
        {
            T obj = PREFAB.InstantiateOrNull<T>();
            if (obj == null)
            {
                throw new ArgumentException($"The given prefab cannot be instantiated into type {typeof(T)}.");
            }

            PARENT_NODE.AddChild(obj);

            // Depending upon the base type, getting it out of the way may be difficult.
            switch (obj)
            {
                case Node2D node2D:
                    node2D.GlobalPosition = new Vector2(-10000f, -10000f);
                    break;
                case Node3D node3D:
                    node3D.GlobalPosition = new Vector3(0f, -10000f, 0f);
                    break;
                case Control control:
                    control.GlobalPosition = new Vector2(-100f, -100f);
                    break;
            }

            return obj;
        }


        /// <summary> Get a new object by pulling from the pool. </summary>
        /// <returns> The now active object. </returns>
        public T CreateObject()
        {
            T result = null;

            // Attempt to find a free object in the pool.
            foreach (KeyValuePair<T, Boolean> obj in _objectPool)
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
                _poolSize++;
            }

            _objectPool[result] = true;

            return result;
        }


        /// <summary> Free an object and return it to the pool. </summary>
        /// <param name="obj"> The object to return. </param>
        public void FreeItem(T obj)
        {
            _objectPool[obj] = false;

            // Depending upon the base type, getting it out of the way may be difficult.
            switch (obj)
            {
                case Node2D node2D:
                    node2D.GlobalPosition = new Vector2(-10000f, -10000f);
                    break;
                case Node3D node3D:
                    node3D.GlobalPosition = new Vector3(0f, -10000f, 0f);
                    break;
                case Control control:
                    control.GlobalPosition = new Vector2(-100f, -100f);
                    break;
            }
        }
    }
}
