using System;
using System.Diagnostics.CodeAnalysis;

namespace Jaypen.Utilities.ECS
{
    /// <summary> Extends <see cref="IComponentHolder{T}"/> for holders that permit at most one component per concrete type. </summary>
    /// <typeparam name="T"> The base type that all components attached to this holder must satisfy. </typeparam>
    /// <remarks>
    /// Under this contract the concrete type acts as its own key, so retrieval and removal are unambiguous
    /// without an explicit identifier.  Attempting to attach a second instance of the same concrete type
    /// leaves the holder unchanged; <see cref="IComponentHolder{T}.AddComponent"/> returns <c>false</c>
    /// in that case.
    /// </remarks>
    public interface ISingleComponentHolder<T> : IComponentHolder<T> where T : class, IComponent
    {
        /// <summary> Attempts to retrieve the single attached component whose concrete runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The exact concrete component type to retrieve; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <param name="component"> Contains the attached component when this method returns <c>true</c>; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the matching component was found; <c>false</c> if none is attached. </returns>
        public Boolean TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : class, T;

        /// <summary> Removes the attached component whose runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The concrete component type to remove; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if none of that type was attached. </returns>
        public Boolean RemoveComponent<TComponent>() where TComponent : class, T;
    }
}
