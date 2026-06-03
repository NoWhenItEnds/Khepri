using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Jaypen.Utilities.Extensions;

namespace Khepri.Prefabs
{
    /// <summary> The write-side counterpart to <see cref="PrefabData"/>: builds a JSON object from typed setters so that part implementations never bind directly to <see cref="System.Text.Json"/>. </summary>
    /// <remarks>
    /// Keys are normalised to snake_case on insertion (using the same rules as <see cref="PrefabData"/>) so that keys written by <see cref="IStatefulPart.WriteState"/> round-trip correctly when the resulting JSON is re-read by <see cref="PrefabData"/> accessors.
    /// Duplicate keys (after normalisation) cause an explicit failure rather than a silent last-write-wins overwrite — the same discipline as <see cref="PrefabData"/>'s ambiguity guard.
    /// Call <see cref="ToJsonObject"/> to retrieve the completed object for serialisation.
    /// The <see cref="Context"/> property carries an opaque caller-supplied object that domain-specific part implementations can use to thread contextual callbacks (for example, an entity-serialise callback for container components) without coupling this generic type to any domain.  The generic prefab infrastructure never reads or writes <see cref="Context"/>.
    /// </remarks>
    public sealed class StateWriter
    {
        /// <summary> The JSON object being assembled, keyed by normalised snake_case property name. </summary>
        private readonly JsonObject _object;

        /// <summary> Maps each normalised snake_case key back to the original key that first inserted it, for use in duplicate-key error messages. </summary>
        private readonly Dictionary<String, String> _originalKeys;

        /// <summary> An opaque caller-supplied context object; set by the entity layer to carry domain-specific callbacks into <see cref="IStatefulPart.WriteState"/> implementations. </summary>
        /// <remarks> This property is never read by the generic prefab infrastructure. The setter is <c>internal</c> to prevent callers outside the assembly from mutating the slot after the writer has been passed into a <see cref="IStatefulPart.WriteState"/> call. </remarks>
        public Object? Context { get; internal set; }


        /// <summary> Initialises a new, empty <see cref="StateWriter"/>. </summary>
        public StateWriter()
        {
            _object       = new JsonObject();
            _originalKeys = new Dictionary<String, String>();
        }


        /// <summary> Records a string property under <paramref name="key"/>. </summary>
        /// <param name="key"> The property name; normalised to snake_case before insertion. </param>
        /// <param name="value"> The string value to record. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="key"/> (after normalisation) has already been set; the message names both original keys. </exception>
        public void SetString(String key, String value)
        {
            InsertProperty(key, JsonValue.Create(value));
        }


        /// <summary> Records a 32-bit integer property under <paramref name="key"/>. </summary>
        /// <param name="key"> The property name; normalised to snake_case before insertion. </param>
        /// <param name="value"> The integer value to record. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="key"/> (after normalisation) has already been set; the message names both original keys. </exception>
        public void SetInt32(String key, Int32 value)
        {
            InsertProperty(key, JsonValue.Create(value));
        }


        /// <summary> Records a single-precision floating-point property under <paramref name="key"/>. </summary>
        /// <param name="key"> The property name; normalised to snake_case before insertion. </param>
        /// <param name="value"> The single value to record. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="key"/> (after normalisation) has already been set; the message names both original keys. </exception>
        public void SetSingle(String key, Single value)
        {
            InsertProperty(key, JsonValue.Create(value));
        }


        /// <summary> Records a boolean property under <paramref name="key"/>. </summary>
        /// <param name="key"> The property name; normalised to snake_case before insertion. </param>
        /// <param name="value"> The boolean value to record. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="key"/> (after normalisation) has already been set; the message names both original keys. </exception>
        public void SetBoolean(String key, Boolean value)
        {
            InsertProperty(key, JsonValue.Create(value));
        }


        /// <summary> Records a raw JSON array property under <paramref name="key"/>. </summary>
        /// <remarks> Intended for domain-specific uses such as embedding serialised nested entities; the caller is responsible for building the array contents. </remarks>
        /// <param name="key"> The property name; normalised to snake_case before insertion. </param>
        /// <param name="value"> The <see cref="JsonArray"/> to record as-is. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="value"/> is null. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="key"/> (after normalisation) has already been set; the message names both original keys. </exception>
        public void SetJsonArray(String key, JsonArray value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "JSON array value must not be null.");
            }

            InsertProperty(key, value);
        }


        /// <summary> Returns the completed JSON object containing all recorded properties. </summary>
        /// <remarks> The returned object is the same instance mutated by the setter calls; callers should not add further properties to it directly. </remarks>
        /// <returns> The assembled <see cref="JsonObject"/>. </returns>
        public JsonObject ToJsonObject()
        {
            return _object;
        }


        /// <summary> Normalises <paramref name="key"/> to snake_case, checks for a duplicate, and inserts <paramref name="node"/> into the object. </summary>
        /// <param name="key"> The caller-supplied property name. </param>
        /// <param name="node"> The JSON node to insert. </param>
        /// <exception cref="InvalidOperationException"> Thrown when the normalised key has already been inserted; the message names both original keys so the collision is unambiguous. </exception>
        private void InsertProperty(String key, JsonNode? node)
        {
            String  normalisedKey = key.ToSnakeCase();
            Boolean collision     = _originalKeys.TryGetValue(normalisedKey, out String? existingOriginal);

            if (collision)
            {
                throw new InvalidOperationException(
                    $"StateWriter: keys '{existingOriginal}' and '{key}' both normalise to '{normalisedKey}'. Remove the duplicate setter call.");
            }

            _originalKeys[normalisedKey] = key;
            _object[normalisedKey]       = node;
        }
    }
}
