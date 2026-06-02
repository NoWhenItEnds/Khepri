using System;
using System.Collections.Generic;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> A reusable, data-driven definition of a standard component set that can be applied to any number of fresh entities without modification. </summary>
    public sealed class EntityPrefab
    {
        /// <summary> The ordered sequence of operations that apply each recorded component factory to a supplied builder. </summary>
        private readonly List<Func<EntityBuilder, EntityBuilder>> _applications;


        /// <summary> Initialises a new, empty prefab for an entity. </summary>
        private EntityPrefab()
        {
            _applications = new List<Func<EntityBuilder, EntityBuilder>>();
        }


        /// <summary> Creates and returns a new empty entity prefab, ready for component definitions to be added. </summary>
        /// <returns> A new <see cref="EntityPrefab"/> instance with no components registered. </returns>
        public static EntityPrefab Define()
        {
            return new EntityPrefab();
        }


        /// <summary> Records a component factory on this prefab so that every entity created from it receives a component of type <typeparamref name="T"/>, and returns this prefab for further chaining. </summary>
        /// <typeparam name="T"> The concrete <see cref="Component"/> type to register. </typeparam>
        /// <param name="factory"> A delegate that receives the entity under construction and returns a fully constructed component. Forwarded verbatim to <see cref="EntityBuilder.WithComponent{T}"/> — must not return <c>null</c>. </param>
        /// <returns> This prefab instance, enabling a fluent call chain. </returns>
        public EntityPrefab WithComponent<T>(Func<Entity, T> factory) where T : Component
        {
            _applications.Add(builder => builder.WithComponent(factory));
            return this;
        }


        /// <summary> Applies every component factory registered on this prefab to the supplied builder, in registration order, and returns the builder for further chaining. </summary>
        /// <remarks> This method is intentionally <c>internal</c> — callers interact via <see cref="EntityFactory.CreateFrom"/>. </remarks>
        /// <param name="builder"> The builder to apply this prefab's component set to. </param>
        /// <returns> The same <paramref name="builder"/> instance after all factories have been applied, ready for additional per-instance components or a final <see cref="EntityBuilder.Build"/> call. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="EntityBuilder.WithComponent{T}"/> when a factory returns <c>null</c> or when a duplicate component type is detected. </exception>
        internal EntityBuilder ApplyTo(EntityBuilder builder)
        {
            EntityBuilder result = builder;

            foreach (Func<EntityBuilder, EntityBuilder> application in _applications)
            {
                result = application(result);
            }

            return result;
        }
    }
}
