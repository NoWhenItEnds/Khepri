using System;
using Khepri.Entities.Components;
using Khepri.Prefabs;

namespace Khepri.Entities
{
    /// <summary> Entry point for constructing new entities via a fluent builder. </summary>
    public static class EntityFactory
    {
        /// <summary> Creates a new entity with a freshly generated unique identifier and returns a builder for attaching components before finalising construction. </summary>
        /// <remarks> The entity is created immediately so components can reference their owning entity at construction time. Call <see cref="EntityBuilder.Build"/> when all components have been registered. </remarks>
        /// <returns> An <see cref="EntityBuilder"/> pre-loaded with the new entity. </returns>
        public static EntityBuilder Create()
        {
            return new EntityBuilder(new Entity(GenerateUId()));
        }


        /// <summary> Begins rebuilding an entity from a known identifier, preserving the original identity rather than generating a fresh one. </summary>
        /// <remarks> This is the low-level seed used by <see cref="EntityReconstructor"/>, which is the registry-holding orchestrator responsible for the full save-driven reconstruction path. Callers that need to restore from a save JSON string should use <see cref="EntityReconstructor.Reconstruct"/> instead. </remarks>
        /// <param name="uid"> The previously assigned identifier the entity must retain. </param>
        /// <returns> An <see cref="EntityBuilder"/> seeded with the supplied identifier. </returns>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="uid"/> is <see cref="Guid.Empty"/>, which is not a valid entity identifier. </exception>
        public static EntityBuilder Reconstruct(Guid uid)
        {
            if (uid == Guid.Empty)
            {
                throw new ArgumentException("An empty GUID is not a valid entity identifier.", nameof(uid));
            }

            return new EntityBuilder(new Entity(uid));
        }


        /// <summary> Creates a fresh entity, applies all component factories defined on <paramref name="prefab"/> in registration order, and returns the builder for additional per-instance components before finalising construction. </summary>
        /// <remarks> Because the prefab's component set is applied first, a subsequent <see cref="EntityBuilder.WithComponent{T}"/> call for the same type will trigger the duplicate guard. The original <see cref="EntityBuilder"/> is returned so callers retain the entity-specific fluent API. </remarks>
        /// <param name="prefab"> The prefab whose component set to apply to the new entity. </param>
        /// <returns> An <see cref="EntityBuilder"/> with the prefab's components already attached, ready for further per-instance additions. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="EntityBuilder.WithComponent{T}"/> when a prefab factory returns <c>null</c> or registers a duplicate component type. </exception>
        public static EntityBuilder CreateFrom(Prefab<Entity, Component> prefab)
        {
            EntityBuilder builder = Create();
            prefab.ApplyTo(builder);
            return builder;
        }


        /// <summary> Generates a new unique identifier for an entity. This is the single point to ensure consistency. </summary>
        /// <returns> A newly generated <see cref="Guid"/>. </returns>
        private static Guid GenerateUId()
        {
            return Guid.NewGuid();  // TODO - Ensure uniqueness across all entities, including those loaded from saves or network sync. May require a central registry of assigned IDs.
        }
    }
}
