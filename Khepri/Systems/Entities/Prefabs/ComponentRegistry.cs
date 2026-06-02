using System;
using System.Collections.Generic;
using Khepri.Entities.Components;

namespace Khepri.Entities.Prefabs
{
    /// <summary> Maps string type-keys to component factories, acting as the Open/Closed extension point for runtime prefab loading — new component types are supported by calling <see cref="Register"/> without modifying any existing loader or parser code. </summary>
    /// <remarks> This class is not thread-safe. All registrations must be completed at the composition root before any thread begins loading prefabs. Each type-key is a short, stable string that identifies a component kind in JSON, e.g. <c>"Health"</c> or <c>"Position"</c>. Keys are matched case-sensitively. </remarks>
    public sealed class ComponentRegistry
    {
        /// <summary> Maps each registered type-key to its component factory delegate. </summary>
        private readonly Dictionary<String, Func<Entity, ComponentData, Component>> _factories;


        /// <summary> Initialises a new, empty registry. </summary>
        public ComponentRegistry()
        {
            _factories = new Dictionary<String, Func<Entity, ComponentData, Component>>();
        }


        /// <summary> Associates <paramref name="typeKey"/> with <paramref name="factory"/> so that the loader can instantiate components of this type from JSON data. </summary>
        /// <param name="typeKey"> The short string identifying this component kind in JSON. Must not be null or whitespace. </param>
        /// <param name="factory"> A delegate that receives the owning entity and the component's parsed JSON data, and returns a fully constructed, non-null component instance. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="typeKey"/> is null, empty, or whitespace. </exception>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="factory"/> is null. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="typeKey"/> is already registered — duplicate registration is a programming error. </exception>
        public void Register(String typeKey, Func<Entity, ComponentData, Component> factory)
        {
            if (String.IsNullOrWhiteSpace(typeKey))
            {
                throw new ArgumentException("Type key must not be null or whitespace.", nameof(typeKey));
            }

            ArgumentNullException.ThrowIfNull(factory);

            Boolean alreadyRegistered = _factories.ContainsKey(typeKey);

            if (alreadyRegistered)
            {
                throw new InvalidOperationException($"A factory for component type '{typeKey}' is already registered. Duplicate registrations are not permitted.");
            }

            _factories[typeKey] = factory;
        }


        /// <summary> Returns <c>true</c> when a factory for <paramref name="typeKey"/> has been registered, <c>false</c> otherwise. Does not throw for unregistered keys — use this to probe without committing to a <see cref="Create"/> call. </summary>
        /// <param name="typeKey"> The component type identifier to test. </param>
        /// <returns> <c>true</c> if a factory is registered for <paramref name="typeKey"/>. </returns>
        public Boolean IsRegistered(String typeKey) => _factories.ContainsKey(typeKey);


        /// <summary> Looks up the factory for <paramref name="typeKey"/>, invokes it with the supplied entity and data, and returns the resulting component. </summary>
        /// <param name="typeKey"> The component type identifier, as it appears in the JSON file. </param>
        /// <param name="entity"> The entity the component will be attached to. </param>
        /// <param name="data"> The parsed JSON payload for this component entry. </param>
        /// <returns> The newly constructed component. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="typeKey"/> has not been registered. The message names the unknown type key so the caller can identify the offending JSON entry. </exception>
        public Component Create(String typeKey, Entity entity, ComponentData data)
        {
            Boolean found = _factories.TryGetValue(typeKey, out Func<Entity, ComponentData, Component>? factory);

            if (!found)
            {
                throw new InvalidOperationException($"No factory is registered for component type '{typeKey}'. Register one via ComponentRegistry.Register before loading prefabs that reference it.");
            }

            return factory!(entity, data);
        }
    }
}
