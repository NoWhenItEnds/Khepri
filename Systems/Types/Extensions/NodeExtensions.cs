using Godot;
using System.Collections.Generic;

namespace Khepri.Types.Extensions
{
    /// <summary> Useful methods for working with generic nodes. </summary>
    public static class NodeExtensions
    {
        /// <summary> Gets all the children and sub-children of a particular type. </summary>
        /// <typeparam name="T"> The type of node to search for. </typeparam>
        /// <param name="node"> The root node to begin. </param>
        /// <returns> An array of all the nodes of a given type. </returns>
        public static T[] GetChildrenOfType<T>(this Node node)
        {
            List<T> results = new List<T>();

            foreach (Node child in node.GetChildren(true))
            {
                if (child is T type)
                {
                    results.Add(type);
                }
                if (child.GetChildCount(true) > 0)
                {
                    results.AddRange(child.GetChildrenOfType<T>());
                }
            }

            return results.ToArray();
        }
    }
}
