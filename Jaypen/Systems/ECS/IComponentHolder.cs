using System;
using System.Collections.Generic;

namespace Jaypen.Utilities.ECS
{
    /// <summary> Cardinality-agnostic core contract for objects that hold components of a specific base type. </summary>
    /// <typeparam name="T"> The base type that all components attached to this holder must satisfy. </typeparam>
    /// <remarks>
    /// This interface makes no assumption about how many components of a given concrete type may coexist.
    /// Cardinality rules are defined by the sub-interfaces <see cref="ISingleComponentHolder{T}"/> and
    /// <see cref="IMultiComponentHolder{T}"/>.
    /// </remarks>
    public interface IComponentHolder<T> where T : class, IComponent
    {
        /// <summary> Raised after a component is successfully attached to this holder; the argument is the component that was just added. </summary>
        /// <remarks> Handlers receive a reference to the attached component. They are invoked synchronously after the holder's internal collection has already been mutated, so the holder's state already reflects the change when handlers run. </remarks>
        public event Action<T>? ComponentAdded;

        /// <summary> Raised after a component is successfully removed from this holder; the argument is the component that was just detached. </summary>
        /// <remarks> Handlers receive a reference to the detached component. They are invoked synchronously after the holder's internal collection has already been mutated, so the holder's state already reflects the change when handlers run. </remarks>
        public event Action<T>? ComponentRemoved;

        /// <summary> Adds a component instance to this holder. </summary>
        /// <param name="component"> The component to attach. Must not be <c>null</c>. </param>
        /// <returns>
        /// <c>true</c> if the component was added; <c>false</c> if the holder rejected it —
        /// the component is guaranteed not to be attached as a result of this call when <c>false</c> is returned.
        /// The specific reasons for rejection are defined by the implementing cardinality contract
        /// (see <see cref="ISingleComponentHolder{T}"/> and <see cref="IMultiComponentHolder{T}"/>).
        /// </returns>
        /// <exception cref="System.ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean AddComponent(T component);

        /// <summary> Returns all components currently attached to this holder, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the holder's internal component set. </remarks>
        /// <returns> A read-only snapshot of every attached component. </returns>
        public IReadOnlyCollection<T> GetComponents();

        /// <summary> Checks whether at least one component matching <typeparamref name="TComponent"/> is currently attached. </summary>
        /// <remarks> The matching semantics follow the implementing cardinality contract: exact-type for <see cref="ISingleComponentHolder{T}"/>, assignability for <see cref="IMultiComponentHolder{T}"/>. </remarks>
        /// <typeparam name="TComponent"> The component type to test for; must satisfy <typeparamref name="T"/>. </typeparam>
        /// <returns> <c>true</c> if at least one matching component is attached; <c>false</c> otherwise. </returns>
        public Boolean HasComponent<TComponent>() where TComponent : T;

        /// <summary> Removes a specific component instance from this holder. </summary>
        /// <param name="component"> The exact component instance to detach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if it was not attached. </returns>
        /// <exception cref="System.ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean RemoveComponent(T component);
    }
}
