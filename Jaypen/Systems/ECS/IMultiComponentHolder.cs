using System;
using System.Collections.Generic;

namespace Jaypen.Utilities.ECS
{
    /// <summary> Extends <see cref="IComponentHolder{T}"/> for holders that permit multiple components of the same concrete type. </summary>
    /// <typeparam name="T"> The base type that all components attached to this holder must satisfy. </typeparam>
    /// <remarks>
    /// Because several components of the same concrete type may coexist, per-type retrieval and removal operate on sets
    /// rather than a single instance, and type matching is by assignability — querying a base type matches every component
    /// derived from it, unlike the exact-type lookups of <see cref="ISingleComponentHolder{T}"/>.
    /// <see cref="IComponentHolder{T}.AddComponent"/> rejects only an instance that is already attached (reference
    /// equality); distinct instances of the same concrete type are always accepted. Instance-level removal is available
    /// via <see cref="IComponentHolder{T}.RemoveComponent(T)"/> on the base interface.
    /// </remarks>
    public interface IMultiComponentHolder<T> : IComponentHolder<T> where T : class, IComponent
    {
        /// <summary> Returns all attached components whose concrete runtime type is assignable to <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The component type to retrieve, including anything derived from it; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <returns> A read-only snapshot of every matching attached component; empty if none are attached. </returns>
        public IReadOnlyCollection<TComponent> GetComponents<TComponent>() where TComponent : T;

        /// <summary> Removes all attached components whose concrete runtime type is assignable to <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The component type to remove, including anything derived from it; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <returns> <c>true</c> if at least one component was removed; <c>false</c> if none of that type were attached. </returns>
        public Boolean RemoveComponents<TComponent>() where TComponent : T;

        /// <summary> Returns the number of attached components whose concrete runtime type is assignable to <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The component type to count, including anything derived from it; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <returns> The count of matching attached components; zero if none are attached. </returns>
        public Int32 CountComponents<TComponent>() where TComponent : T;
    }
}
