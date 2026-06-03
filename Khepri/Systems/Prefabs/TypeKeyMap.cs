using System;
using System.Collections.Generic;
using Jaypen.Utilities.Extensions;

namespace Khepri.Prefabs
{
    /// <summary> Maps the runtime <see cref="Type"/> of a registered part to the string type-key used to identify it in JSON. </summary>
    /// <remarks>
    /// Populated during startup by <see cref="FactoryDiscovery{TOwner,TPart}.RegisterAll(FactoryRegistry{TOwner,TPart}, TypeKeyMap)"/> as part of the single assembly scan.
    /// The serialiser consults this map when writing save data: given a live component's <see cref="Object.GetType"/>, it looks up the key to emit as the <c>"type"</c> field.
    /// Not thread-safe. All entries must be added at the composition root before any serialisation begins.
    /// </remarks>
    public sealed class TypeKeyMap
    {
        /// <summary> Maps each registered part type to its normalised snake_case type-key. </summary>
        private readonly Dictionary<Type, String> _typeToKey;


        /// <summary> Initialises a new, empty map. </summary>
        public TypeKeyMap()
        {
            _typeToKey = new Dictionary<Type, String>();
        }


        /// <summary> Records a mapping from <paramref name="type"/> to <paramref name="typeKey"/>, normalising the key to snake_case before storage. </summary>
        /// <remarks> Normalisation mirrors <see cref="FactoryRegistry{TOwner,TPart}.Register"/> so that the key emitted by <see cref="Resolve"/> matches the key used by the registry at reconstruction time. </remarks>
        /// <param name="type"> The runtime type of the part class; must not be null. </param>
        /// <param name="typeKey"> The raw type-key (e.g. <c>"Health"</c>); normalised to snake_case (e.g. <c>"health"</c>) before storage. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="type"/> is null. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="type"/> has already been mapped; the message names the conflicting type. </exception>
        internal void Register(Type type, String typeKey)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type), "Part type must not be null.");
            }

            String  normalisedKey = typeKey.ToSnakeCase();
            Boolean alreadyMapped = _typeToKey.ContainsKey(type);

            if (alreadyMapped)
            {
                throw new InvalidOperationException(
                    $"TypeKeyMap: type '{type.FullName}' has already been mapped to key '{_typeToKey[type]}'. Duplicate registrations are not permitted.");
            }

            _typeToKey[type] = normalisedKey;
        }


        /// <summary> Returns the type-key registered for <paramref name="type"/>. </summary>
        /// <remarks> Every part type that may be serialised must declare a <c>[ComponentFactory]</c> (or equivalent factory) method; <see cref="FactoryDiscovery{TOwner,TPart}"/> populates this map from those methods, so a part with no factory will never appear here. </remarks>
        /// <param name="type"> The runtime type to look up. </param>
        /// <returns> The normalised snake_case type-key associated with <paramref name="type"/>. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when <paramref name="type"/> has not been registered; the message names the type so the caller can identify the authoring gap. </exception>
        public String Resolve(Type type)
        {
            Boolean found = _typeToKey.TryGetValue(type, out String? key);

            if (!found)
            {
                throw new KeyNotFoundException(
                    $"TypeKeyMap: no type-key is registered for '{type.FullName}'. Ensure a [ComponentFactory] (or equivalent spawn factory) method is declared on this type and that RegisterAll has been called.");
            }

            return key!;
        }
    }
}
