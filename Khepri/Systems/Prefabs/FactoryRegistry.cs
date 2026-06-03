using System;
using System.Collections.Generic;
using Jaypen.Utilities.Extensions;

namespace Khepri.Prefabs
{
    /// <summary> Maps string type-keys to part factories, acting as the Open/Closed extension point for runtime prefab loading — new part types are supported by calling <see cref="Register"/> without modifying any existing loader or parser code. </summary>
    /// <remarks>
    /// Not thread-safe. All registrations must be completed at the composition root before any thread begins loading prefabs.
    /// Callers retrieve a resolved factory delegate via <see cref="Resolve"/> at load time, so the registry is never consulted at owner-creation time.
    /// </remarks>
    /// <typeparam name="TOwner"> The type of the owning object that factories receive. </typeparam>
    /// <typeparam name="TPart"> The base type that all registered factories produce. </typeparam>
    public class FactoryRegistry<TOwner, TPart>
    {
        /// <summary> Maps each normalised snake_case type-key to its part factory delegate. </summary>
        private readonly Dictionary<String, Func<TOwner, PrefabData, TPart>> _factories;


        /// <summary> Initialises a new, empty registry. </summary>
        public FactoryRegistry()
        {
            _factories = new Dictionary<String, Func<TOwner, PrefabData, TPart>>();
        }


        /// <summary> Associates <paramref name="typeKey"/> with <paramref name="factory"/> so that the loader can instantiate parts of this type from JSON data. </summary>
        /// <param name="typeKey"> The string identifying this part kind in JSON; normalised to snake_case before storage. Must not be null or whitespace. </param>
        /// <param name="factory"> A delegate that receives the owning object and the part's parsed JSON data, and returns a fully constructed, non-null part instance. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="typeKey"/> is null, empty, or whitespace, or when it normalises to an empty string. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the normalised form of <paramref name="typeKey"/> is already registered — the message names both the original key and its normalised form so the collision is unambiguous. </exception>
        public void Register(String typeKey, Func<TOwner, PrefabData, TPart> factory)
        {
            if (String.IsNullOrWhiteSpace(typeKey))
            {
                throw new ArgumentException("Type key must not be null or whitespace.", nameof(typeKey));
            }

            String normalisedKey      = typeKey.ToSnakeCase();
            Boolean alreadyRegistered = _factories.ContainsKey(normalisedKey);

            if (alreadyRegistered)
            {
                throw new InvalidOperationException($"A factory for part type '{typeKey}' (normalised: '{normalisedKey}') is already registered. Duplicate registrations are not permitted.");
            }

            _factories[normalisedKey] = factory;
        }


        /// <summary> Returns the factory delegate registered for <paramref name="typeKey"/>, to be captured and invoked later at owner-creation time. </summary>
        /// <remarks> Normalises <paramref name="typeKey"/> to snake_case before lookup. Resolving at load time means any missing registration is caught immediately — before any owner is created from the prefab. </remarks>
        /// <param name="typeKey"> The part type identifier as it appears in the JSON file; normalised to snake_case before lookup. </param>
        /// <returns> The factory delegate bound to the normalised key; the caller is responsible for invoking it with the owning object and part data. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the normalised form of <paramref name="typeKey"/> has not been registered. The message names both the original key and its normalised form so the caller can identify the offending JSON entry, and advises registration via <see cref="Register"/>. </exception>
        public Func<TOwner, PrefabData, TPart> Resolve(String typeKey)
        {
            Boolean found = TryResolve(typeKey, out Func<TOwner, PrefabData, TPart>? factory);

            if (!found)
            {
                String normalisedKey = typeKey.ToSnakeCase();
                throw new InvalidOperationException($"No factory is registered for part type '{typeKey}' (normalised: '{normalisedKey}'). Register one via FactoryRegistry.Register before loading prefabs that reference it.");
            }

            return factory!;
        }


        /// <summary> Attempts to retrieve the factory delegate registered for <paramref name="typeKey"/> without throwing when the key is absent. </summary>
        /// <remarks> Normalises <paramref name="typeKey"/> to snake_case before lookup, matching the normalisation applied by <see cref="Register"/>. Use this method when an absent key should produce a domain-specific error message — for example, <see cref="EntityReconstructor"/> uses it to report an unregistered component type-key with full save-path context rather than the generic registry message. </remarks>
        /// <param name="typeKey"> The part type identifier; normalised to snake_case before lookup. </param>
        /// <param name="factory"> When this method returns <c>true</c>, the factory delegate bound to the normalised key; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> when a factory is registered for the normalised key; <c>false</c> when it is absent. </returns>
        public Boolean TryResolve(String typeKey, out Func<TOwner, PrefabData, TPart>? factory)
        {
            String  normalisedKey = typeKey.ToSnakeCase();
            Boolean found         = _factories.TryGetValue(normalisedKey, out factory);
            return found;
        }
    }
}
