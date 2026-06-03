using System;
using Khepri.Entities.Components;
using Khepri.Prefabs;

namespace Khepri.Entities
{
    /// <summary> Fluent builder for constructing an <see cref="Entity"/> and attaching components in a single expression. </summary>
    /// <remarks> Obtain an instance via <see cref="EntityFactory.Create()"/> — do not construct directly. Extends <see cref="PrefabBuilder{TOwner,TPart}"/> so that a <c>Prefab&lt;Entity, Component&gt;</c> can apply itself via the generic <c>ApplyTo</c> path, whilst the <c>WithComponent</c> methods preserve the public API for direct callers. </remarks>
    public sealed class EntityBuilder : PrefabBuilder<Entity, Component>
    {
        /// <summary> Initialises a new builder that will assemble the supplied entity. </summary>
        /// <param name="entity"> The entity to attach components to. </param>
        internal EntityBuilder(Entity entity) : base(entity)
        {
        }


        /// <summary> Invokes <paramref name="factory"/> with the entity under construction, attaches the produced component, and returns this builder for further chaining. </summary>
        /// <typeparam name="T"> The concrete <see cref="Component"/> type being added; used to name the type in error messages. </typeparam>
        /// <param name="factory"> A delegate that receives the entity and returns a fully constructed component. Must not return <c>null</c>. </param>
        /// <returns> This builder instance, enabling a fluent call chain. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="factory"/> returns <c>null</c>, or when a duplicate component type is already attached to the entity. </exception>
        public EntityBuilder WithComponent<T>(Func<Entity, T> factory) where T : Component
        {
            base.WithPart<T>(factory);
            return this;
        }


        /// <summary> Invokes <paramref name="factory"/> with the entity under construction, attaches the produced component, and returns this builder for further chaining. </summary>
        /// <remarks> Non-generic overload for the prefab path; errors report the component's runtime type rather than a compile-time parameter. </remarks>
        /// <param name="factory"> A delegate that receives the entity and returns a fully constructed component. Must not return <c>null</c>. </param>
        /// <returns> This builder instance, enabling a fluent call chain. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="factory"/> returns <c>null</c>, or when a duplicate component type is already attached to the entity. </exception>
        public EntityBuilder WithComponent(Func<Entity, Component> factory)
        {
            base.WithPart(factory);
            return this;
        }
    }
}
