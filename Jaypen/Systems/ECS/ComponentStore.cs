using System;
using System.Collections.Generic;

namespace Jaypen.Utilities.ECS
{
    /// <summary> A reusable storage helper that manages a type-keyed collection of components — at most one per exact concrete type. </summary>
    /// <remarks>
    /// This class encapsulates the dictionary and all CRUD operations so that holders need not duplicate them.
    /// It carries no events and no owner concept; those responsibilities stay on the holder that wraps this store.
    /// </remarks>
    /// <typeparam name="TComponent"> The base type every stored component must satisfy. </typeparam>
    public sealed class ComponentStore<TComponent> where TComponent : IComponent
    {
        /// <summary> The internal dictionary keyed by each component's exact concrete runtime type. </summary>
        private readonly Dictionary<Type, TComponent> _components = new Dictionary<Type, TComponent>();


        /// <summary> Attempts to add a component to the store. </summary>
        /// <param name="component"> The component to add; must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was added; <c>false</c> if a component of the same exact concrete type is already present — the existing entry is left unchanged. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean Add(TComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            return _components.TryAdd(component.GetType(), component);
        }


        /// <summary> Returns all components currently held in the store, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the store's internal state. </remarks>
        /// <returns> A read-only snapshot of every held component. </returns>
        public IReadOnlyCollection<TComponent> GetAll()
        {
            return new List<TComponent>(_components.Values);
        }


        /// <summary> Checks whether the store holds a component whose exact concrete runtime type is <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The exact concrete component type to test for; must satisfy <typeparamref name="TComponent"/>. </typeparam>
        /// <returns> <c>true</c> if a component of that exact type is held; <c>false</c> otherwise. </returns>
        public Boolean Has<T>() where T : TComponent
        {
            return _components.ContainsKey(typeof(T));
        }


        /// <summary> Attempts to retrieve the held component whose exact concrete runtime type is <typeparamref name="T"/>. </summary>
        /// <remarks>
        /// The lookup is exact-type — a subclass of <typeparamref name="T"/> stored under its own key will not be found here.
        /// The <see cref="Boolean"/> return value is the authoritative presence indicator; the out parameter is <c>default</c>
        /// on a miss. The cast from <typeparamref name="TComponent"/> to <typeparamref name="T"/> is safe because the value is
        /// stored under its exact concrete type and <typeparamref name="T"/> is that same type.
        /// </remarks>
        /// <typeparam name="T"> The exact concrete component type to retrieve; must satisfy <typeparamref name="TComponent"/>. </typeparam>
        /// <param name="component"> Contains the held component when this method returns <c>true</c>; otherwise <c>default(<typeparamref name="T"/>)</c>. </param>
        /// <returns> <c>true</c> if the matching component was found; <c>false</c> if none is held. </returns>
        public Boolean TryGet<T>(out T component) where T : TComponent
        {
            Boolean found = _components.TryGetValue(typeof(T), out TComponent? raw);
            component = found ? (T)(Object)raw! : default!;
            return found;
        }


        /// <summary> Removes the held component only if the supplied instance is the exact instance stored under its concrete type. </summary>
        /// <remarks> Uses reference equality — a different instance of the same concrete type will not be removed. </remarks>
        /// <param name="component"> The exact component instance to remove; must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if it was not held or a different instance of the same type is held. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean Remove(TComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            Boolean present = _components.TryGetValue(component.GetType(), out TComponent? attached) && ReferenceEquals(attached, component);
            Boolean removed = present && _components.Remove(component.GetType());
            return removed;
        }


        /// <summary> Removes the held component whose exact concrete runtime type is <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The exact concrete component type to remove; must satisfy <typeparamref name="TComponent"/>. </typeparam>
        /// <param name="removed"> Contains the removed component instance when this method returns <c>true</c>; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if no component of that exact type was held. </returns>
        public Boolean Remove<T>(out TComponent? removed) where T : TComponent
        {
            return _components.Remove(typeof(T), out removed);
        }
    }
}
