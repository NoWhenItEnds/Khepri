using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;

namespace Jaypen.Extensions
{
    /// <summary> Helpful methods for traversing the scene tree. </summary>
    public static class NodeExtensions
    {
        /// <summary> Indicates whether the node can still be used safely: non-null, its native object has not been freed, and it is not queued for deletion at the end of the current frame. </summary>
        /// <param name="node"> The node to check; may be null. </param>
        /// <returns> If the node is safe to touch. </returns>
        /// <remarks> A C# wrapper can outlive its native Godot object, so a plain null check passes while the engine-side node is already gone — touching it then throws <see cref="ObjectDisposedException"/>. Use this before dereferencing any node held across frames or deferred/async boundaries. </remarks>
        public static Boolean IsAlive([NotNullWhen(true)] this Node? node) =>
            node != null && GodotObject.IsInstanceValid(node) && !node.IsQueuedForDeletion();


        /// <summary> Queues every direct child of <paramref name="node"/> for deletion at the end of the current frame. </summary>
        /// <param name="node"> The node whose children are freed; the node itself is untouched. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="node"/> is null. </exception>
        /// <remarks> <see cref="Node.GetChildren"/> returns a snapshot and <see cref="Node.QueueFree"/> defers the removal, so iterating while freeing is safe. </remarks>
        public static void QueueFreeChildren(this Node node)
        {
            ArgumentNullException.ThrowIfNull(node);

            foreach (Node child in node.GetChildren())
            {
                child.QueueFree();
            }
        }



        /// <summary> Collects every descendant of <paramref name="node"/> that is assignable to <typeparamref name="T"/>, in depth-first tree order. </summary>
        /// <typeparam name="T"> The node type or interface to match. </typeparam>
        /// <param name="node"> The node whose subtree is searched; the node itself is not a candidate. </param>
        /// <param name="recursive"> When <see langword="false"/>, only direct children are considered. </param>
        /// <returns> All matching descendants; empty when none match. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="node"/> is null. </exception>
        public static List<T> GetChildrenOfType<T>(this Node node, Boolean recursive = true)
        {
            ArgumentNullException.ThrowIfNull(node);

            List<T> results = new List<T>();
            foreach (Node child in node.GetChildren())
            {
                if (child is T match)
                {
                    results.Add(match);
                }

                if (recursive)
                {
                    results.AddRange(child.GetChildrenOfType<T>(recursive));
                }
            }

            return results;
        }


        /// <summary> Walks up the tree from <paramref name="node"/> and returns the first ancestor assignable to <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The node type or interface to match; must be a reference type. </typeparam>
        /// <param name="node"> The node whose ancestors are searched; the node itself is not a candidate. </param>
        /// <returns> The nearest matching ancestor, or null when none exists. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="node"/> is null. </exception>
        public static T? FindAncestor<T>(this Node node) where T : class
        {
            ArgumentNullException.ThrowIfNull(node);

            T? found = null;
            Node? current = node.GetParent();

            while (current != null && found == null)
            {
                found = current as T;
                current = current.GetParent();
            }

            return found;
        }
    }
}
