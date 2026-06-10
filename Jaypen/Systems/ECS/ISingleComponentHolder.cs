using System;

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
    public interface ISingleComponentHolder<T> : IComponentHolder<T> where T : IComponent
    {
        /// <summary> Attempts to retrieve the single attached component whose concrete runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The exact concrete component type to retrieve; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <param name="component">
        /// When this method returns <c>true</c>, contains the attached component; otherwise <c>default(<typeparamref name="TComponent"/>)</c>.
        /// Because <typeparamref name="TComponent"/> may be a value type, <c>default</c> is a valid-looking zero value rather than a sentinel —
        /// callers must use the Boolean return value, not this parameter, to determine whether a component is present.
        /// </param>
        /// <returns> <c>true</c> if the matching component was found; <c>false</c> if none is attached. </returns>
        public Boolean TryGetComponent<TComponent>(out TComponent component) where TComponent : T;

        /// <summary> Removes the attached component whose runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The concrete component type to remove; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if none of that type was attached. </returns>
        public Boolean RemoveComponent<TComponent>() where TComponent : T;
    }
}
