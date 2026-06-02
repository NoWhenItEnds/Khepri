using System;

namespace Khepri.Entities
{
    /// <summary> Entry point for constructing new entities via a fluent builder. </summary>
    public static class EntityFactory
    {
        /// <summary> Creates a new entity with a freshly generated unique identifier and returns a builder for attaching components before finalising construction. </summary>
        /// <remarks> The entity is created here so that <see cref="EntityBuilder.WithComponent{T}"/> can pass it immediately to each component factory — components require their owning entity at construction time. Call <see cref="EntityBuilder.Build"/> when all components have been registered. </remarks>
        /// <returns> An <see cref="EntityBuilder"/> pre-loaded with the new entity. </returns>
        public static EntityBuilder Create()
        {
            return new EntityBuilder(new Entity(GenerateUId()));
        }


        /// <summary> Begins rebuilding an entity from a known identifier, preserving the original identity rather than generating a fresh one. Intended for loading from a save file or network sync. </summary>
        /// <param name="uid"> The previously assigned identifier the entity must retain. </param>
        /// <returns> An <see cref="EntityBuilder"/> seeded with the supplied identifier. </returns>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="uid"/> is <see cref="Guid.Empty"/>, which is not a valid entity identifier. </exception>
        public static EntityBuilder Reconstruct(Guid uid)
        {
            if (uid == Guid.Empty)  // TODO - This should use a save json to reconstruct. Not an uid.
            {
                throw new ArgumentException("An empty GUID is not a valid entity identifier.", nameof(uid));
            }

            return new EntityBuilder(new Entity(uid));
        }


        /// <summary> Creates a fresh entity, applies all component factories defined on <paramref name="prefab"/> in registration order, and returns the builder for additional per-instance components before finalising construction. </summary>
        /// <remarks> Because the prefab's component set is applied first, any caller-supplied component of the same type added via a subsequent <see cref="EntityBuilder.WithComponent{T}"/> call will trigger the duplicate guard and throw <see cref="InvalidOperationException"/>. Chain further <see cref="EntityBuilder.WithComponent{T}"/> calls on the returned builder for per-instance variation, then call <see cref="EntityBuilder.Build"/>. </remarks>
        /// <param name="prefab"> The prefab whose component set to apply to the new entity. </param>
        /// <returns> An <see cref="EntityBuilder"/> with the prefab's components already attached, ready for further per-instance additions. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="EntityBuilder.WithComponent{T}"/> when a prefab factory returns <c>null</c> or registers a duplicate component type. </exception>
        public static EntityBuilder CreateFrom(EntityPrefab prefab)
        {
            return prefab.ApplyTo(Create());
        }


        /// <summary> Generates a new unique identifier for an entity. This is the single point to ensure consistency. </summary>
        /// <returns> A newly generated <see cref="Guid"/>. </returns>
        private static Guid GenerateUId()
        {
            return Guid.NewGuid();  // TODO - Ensure uniqueness across all entities, including those loaded from saves or network sync. May require a central registry of assigned IDs.
        }
    }
}
