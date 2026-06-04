using Godot;
using System;
using System.Collections.Generic;

namespace Jaypen.Utilities.Pooling
{
    /// <summary>
    /// A reusable pool of <typeparamref name="T"/> nodes parented under a single host, avoiding the cost of
    /// repeatedly freeing and re-instancing nodes. Acquired nodes are activated (made visible); idle nodes are
    /// deactivated (hidden) and kept for reuse rather than freed.
    /// </summary>
    /// <remarks>
    /// Two usage styles are supported and may be mixed:
    /// <list type="bullet">
    /// <item> Long-lived: <see cref="Acquire"/> a node, then later <see cref="Release"/> it (e.g. projectiles). </item>
    /// <item> Immediate-mode rebuild: <see cref="ReleaseAll"/> then re-<see cref="Acquire"/> every node each pass
    ///        (e.g. a UI list rebuilt whenever its source data changes). </item>
    /// </list>
    /// Activation toggles <see cref="CanvasItem.Visible"/> or <see cref="Node3D.Visible"/> where applicable;
    /// non-visual nodes are simply left parented and idle. The pool is not thread-safe.
    /// </remarks>
    /// <typeparam name="T"> The kind of node the pool hands out. </typeparam>
    public sealed class NodePool<T> where T : Node
    {
        /// <summary> The node every pooled instance is parented to. </summary>
        private readonly Node _host;

        /// <summary> Produces a fresh instance when the pool has no idle one to hand out. </summary>
        private readonly Func<T> _factory;

        /// <summary> Optional hook run as a node returns to the pool, e.g. to reset its state. </summary>
        private readonly Action<T>? _onRelease;

        /// <summary> Deactivated instances available to be handed out again. </summary>
        private readonly Stack<T> _idle = new Stack<T>();

        /// <summary> Instances currently handed out. </summary>
        private readonly List<T> _active = new List<T>();


        /// <summary> Initialises a new pool whose nodes are parented under <paramref name="host"/>. </summary>
        /// <param name="host"> The node newly created instances are added to as children. </param>
        /// <param name="factory"> Produces a new instance when no idle one is available. </param>
        /// <param name="onRelease"> Optional hook run as a node returns to the pool, e.g. to reset its state. </param>
        public NodePool(Node host, Func<T> factory, Action<T>? onRelease = null)
        {
            _host      = host;
            _factory   = factory;
            _onRelease = onRelease;
        }


        /// <summary> The number of nodes currently handed out. </summary>
        public Int32 ActiveCount => _active.Count;


        /// <summary> Hands out a node, reusing an idle one when possible or creating a new child otherwise. </summary>
        /// <returns> An activated node ready for use. </returns>
        public T Acquire()
        {
            T node = _idle.Count > 0 ? _idle.Pop() : Create();
            _active.Add(node);
            SetActive(node, true);
            return node;
        }


        /// <summary> Returns a previously acquired node to the pool, deactivating it. Ignored if the node is not currently active. </summary>
        /// <param name="node"> The node to return. </param>
        public void Release(T node)
        {
            if (_active.Remove(node))
            {
                Deactivate(node);
            }
        }


        /// <summary> Returns every active node to the pool in one pass; ideal for immediate-mode rebuilds. </summary>
        public void ReleaseAll()
        {
            foreach (T node in _active)
            {
                Deactivate(node);
            }

            _active.Clear();
        }


        /// <summary> Instantiates a new node via the factory and parents it under the host. </summary>
        /// <returns> The newly created, parented node. </returns>
        private T Create()
        {
            T node = _factory();
            _host.AddChild(node);
            return node;
        }


        /// <summary> Runs the release hook, hides the node, and moves it to the idle set. </summary>
        /// <param name="node"> The node being deactivated. </param>
        private void Deactivate(T node)
        {
            _onRelease?.Invoke(node);
            SetActive(node, false);
            _idle.Push(node);
        }


        /// <summary> Shows or hides <paramref name="node"/> when it is a visual node; a no-op for non-visual nodes. </summary>
        /// <param name="node"> The node to show or hide. </param>
        /// <param name="active"> <c>true</c> to show; <c>false</c> to hide. </param>
        private static void SetActive(Node node, Boolean active)
        {
            switch (node)
            {
                case CanvasItem canvasItem:
                    canvasItem.Visible = active;
                    break;

                case Node3D node3D:
                    node3D.Visible = active;
                    break;
            }
        }
    }
}
