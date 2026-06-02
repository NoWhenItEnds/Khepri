using System;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> Fluent builder for constructing an <see cref="Entity"/> and attaching components in a single expression. </summary>
    /// <remarks> Obtain an instance via <see cref="EntityFactory.Create()"/> — do not construct directly. </remarks>
    public sealed class EntityBuilder
    {
        /// <summary> The entity being assembled by this builder. </summary>
        private readonly Entity _entity;


        /// <summary> Initialises a new builder that will assemble the supplied entity. </summary>
        /// <param name="entity"> The entity to attach components to. </param>
        internal EntityBuilder(Entity entity)
        {
            _entity = entity;
        }


        /// <summary> Invokes <paramref name="factory"/> with the entity under construction, attaches the produced component, and returns this builder for further chaining. </summary>
        /// <typeparam name="T"> The concrete <see cref="Component"/> type being added. </typeparam>
        /// <param name="factory"> A delegate that receives the entity and returns a fully constructed component. Must not return <c>null</c>. </param>
        /// <returns> This builder instance, enabling a fluent call chain. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="factory"/> returns <c>null</c>, or when <see cref="Entity.AddComponent"/> reports that the component could not be attached (duplicate component of the same type already exists). </exception>
        public EntityBuilder WithComponent<T>(Func<Entity, T> factory) where T : Component
        {
            T component = factory(_entity);

            if (component is null)
            {
                throw new InvalidOperationException($"The factory delegate for component type '{typeof(T).Name}' returned null.");
            }

            Boolean added = _entity.AddComponent(component);
            if (!added)
            {
                throw new InvalidOperationException($"A component of type '{typeof(T).Name}' is already attached to entity '{_entity.UId}'.");
            }

            return this;
        }


        /// <summary> Returns the fully assembled entity, completing the build. </summary>
        /// <returns> The entity with all components attached. </returns>
        public Entity Build()
        {
            return _entity;
        }
    }
}
