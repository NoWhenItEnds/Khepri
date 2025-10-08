using System;
using Khepri.Nodes.Singletons;

namespace Khepri.Resources
{
    /// <summary> The central controller for all the game's entity resources. </summary>
    public partial class ResourceController : SingletonNode<ResourceController>
    {
        /// <summary> Create an instance of an entity resource of the given kind. </summary>
        /// <typeparam name="T"> The type of resource to generate. </typeparam>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <returns> The generated resource, or a null if one couldn't be found. </returns>
        public T? CreateResource<T>(String kind) where T : EntityResource
        {
            return null;
        }
    }
}
