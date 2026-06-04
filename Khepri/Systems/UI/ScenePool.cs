using Godot;
using System;
using System.Collections.Generic;

namespace Khepri.UI
{
    /// <summary>
    /// A high-water-mark pool of reusable <typeparamref name="T"/> nodes, all parented under a single host.
    /// Instead of freeing and re-instancing nodes on every rebuild, callers bracket a pass with <see cref="Begin"/>
    /// and <see cref="End"/> and request nodes via <see cref="Acquire"/>; instances left over from a larger previous
    /// pass are hidden rather than freed, ready to be handed back out on the next pass.
    /// </summary>
    /// <typeparam name="T"> The kind of node the pool hands out. </typeparam>
    public sealed class ScenePool<T> where T : Node
    {
        /// <summary> The node every pooled instance is parented to. </summary>
        private readonly Node _host;

        /// <summary> Produces a fresh instance when the pool has no idle one to hand out. </summary>
        private readonly Func<T> _factory;

        /// <summary> Every instance the pool has created, active and idle alike, in stable creation order. </summary>
        private readonly List<T> _instances = new List<T>();

        /// <summary> The number of instances handed out since the most recent <see cref="Begin"/>. </summary>
        private Int32 _activeCount;


        /// <summary> Initialises a new pool whose instances are parented under <paramref name="host"/>. </summary>
        /// <param name="host"> The node newly created instances are added to as children. </param>
        /// <param name="factory"> Produces a new instance when no idle one is available. </param>
        public ScenePool(Node host, Func<T> factory)
        {
            _host    = host;
            _factory = factory;
        }


        /// <summary> Begins a rebuild pass, making every pooled instance available for reuse. </summary>
        public void Begin() => _activeCount = 0;


        /// <summary> Hands out an instance, reusing an idle one when possible or creating a new child otherwise. </summary>
        /// <returns> A visible instance ready to be positioned and configured by the caller. </returns>
        public T Acquire()
        {
            if (_activeCount == _instances.Count)
            {
                T created = _factory();
                _host.AddChild(created);
                _instances.Add(created);
            }

            T instance = _instances[_activeCount];
            _activeCount++;

            if (instance is CanvasItem canvasItem)
            {
                canvasItem.Visible = true;
            }

            return instance;
        }


        /// <summary> Ends a rebuild pass, hiding any instances left over from a larger previous pass. </summary>
        public void End()
        {
            for (Int32 i = _activeCount; i < _instances.Count; i++)
            {
                if (_instances[i] is CanvasItem canvasItem)
                {
                    canvasItem.Visible = false;
                }
            }
        }
    }
}
