using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Jaypen.Utilities.ECS
{
    /// <summary> Base class for holders that permit at most one component per exact concrete type. </summary>
    /// <remarks>
    /// Derived holders supply only the binding behaviour — <see cref="Bind"/> and <see cref="Unbind"/> — which typically
    /// set and clear the component's typed owner reference. All storage, event raising, and the ordering guarantees
    /// documented on <see cref="IComponentHolder{T}"/> are implemented once here.
    /// </remarks>
    /// <typeparam name="TComponent"> The base type that all components attached to this holder must satisfy. </typeparam>
    public abstract class SingleComponentHolder<TComponent> : ISingleComponentHolder<TComponent>
        where TComponent : class, IComponent
    {
        /// <summary> All components attached to this holder, keyed by each component's exact concrete runtime type. </summary>
        private readonly Dictionary<Type, TComponent> _components = new Dictionary<Type, TComponent>();


        /// <inheritdoc/>
        public event Action<TComponent>? ComponentAdded;


        /// <inheritdoc/>
        public event Action<TComponent>? ComponentRemoved;


        /// <summary> Binds a freshly added component to this holder — typically by setting the component's owner reference. </summary>
        /// <remarks> Called after the component has entered the holder but before <see cref="ComponentAdded"/> is raised. May throw to veto the add (for example, when the component is already bound to a different holder); the add is then rolled back so the holder never retains a component it does not own. </remarks>
        /// <param name="component"> The component that was just added. </param>
        protected abstract void Bind(TComponent component);


        /// <summary> Unbinds a freshly removed component from this holder — typically by clearing the component's owner reference. </summary>
        /// <remarks> Called after the component has left the holder but before <see cref="ComponentRemoved"/> is raised, so observers see an already-unbound component. </remarks>
        /// <param name="component"> The component that was just removed. </param>
        protected abstract void Unbind(TComponent component);


        /// <summary> Adds a component instance to this holder. </summary>
        /// <param name="component"> The component to attach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was added; <c>false</c> if a component of the same concrete type is already attached — the existing component is left unchanged. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="Bind"/> before raising <see cref="ComponentAdded"/>, so observers always see a fully
        /// bound component. If <see cref="Bind"/> throws, the add is rolled back and the exception propagates — the holder
        /// is left unchanged.
        /// </remarks>
        public Boolean AddComponent(TComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            Boolean added = _components.TryAdd(component.GetType(), component);
            if (added)
            {
                try
                {
                    Bind(component);
                }
                catch
                {
                    _components.Remove(component.GetType());
                    throw;
                }
                ComponentAdded?.Invoke(component);
            }
            return added;
        }


        /// <summary> Returns all components currently attached to this holder, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the holder's internal component set. </remarks>
        /// <returns> A read-only snapshot of every attached component. </returns>
        public IReadOnlyCollection<TComponent> GetComponents() => new List<TComponent>(_components.Values);


        /// <summary> Checks whether a component whose concrete runtime type is exactly <typeparamref name="T"/> is currently attached. </summary>
        /// <typeparam name="T"> The exact concrete component type to test for. </typeparam>
        /// <returns> <c>true</c> if a component of that exact type is attached; <c>false</c> otherwise. </returns>
        public Boolean HasComponent<T>() where T : TComponent => _components.ContainsKey(typeof(T));


        /// <summary> Attempts to retrieve the attached component whose concrete runtime type is exactly <typeparamref name="T"/>. </summary>
        /// <remarks>
        /// Uses an exact-type dictionary lookup, not assignability scanning. A subclass of <typeparamref name="T"/> stored under its
        /// own key will not be found here; to search by base type or interface, filter <see cref="GetComponents"/> instead.
        /// </remarks>
        /// <typeparam name="T"> The exact concrete component type to retrieve. </typeparam>
        /// <param name="component"> Contains the attached component when this method returns <c>true</c>; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the matching component was found; <c>false</c> if none is attached. </returns>
        public Boolean TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class, TComponent
        {
            _components.TryGetValue(typeof(T), out TComponent? raw);
            component = raw as T;
            return component is not null;
        }


        /// <summary> Removes the attached component whose runtime type is exactly <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The type of component to remove. </typeparam>
        /// <returns> <c>true</c> if a component was removed; <c>false</c> if none was found. </returns>
        /// <remarks> On success: calls <see cref="Unbind"/> before raising <see cref="ComponentRemoved"/>, so observers see an already-unbound component. </remarks>
        public Boolean RemoveComponent<T>() where T : class, TComponent
        {
            Boolean removed = _components.Remove(typeof(T), out TComponent? removedComponent);
            if (removed) { Unbind(removedComponent!); ComponentRemoved?.Invoke(removedComponent!); }
            return removed;
        }


        /// <summary> Removes a specific component instance from this holder. </summary>
        /// <remarks>
        /// Uses reference equality — a different instance of the same concrete type will not be removed.
        /// On success: calls <see cref="Unbind"/> before raising <see cref="ComponentRemoved"/>, so observers see an already-unbound component.
        /// </remarks>
        /// <param name="component"> The exact component instance to detach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if it was not attached or a different instance of the same type is attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean RemoveComponent(TComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            Boolean present = _components.TryGetValue(component.GetType(), out TComponent? attached) && ReferenceEquals(attached, component);
            Boolean removed = present && _components.Remove(component.GetType());
            if (removed) { Unbind(component); ComponentRemoved?.Invoke(component); }
            return removed;
        }
    }
}
