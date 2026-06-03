using System;
using System.Collections.Generic;
using System.Text.Json;
using Jaypen.Utilities.Extensions;

namespace Khepri.Prefabs
{
    /// <summary> A read-only payload wrapper over a single part's parsed JSON object, providing typed, fail-explicit accessors so that part factories never bind directly to <see cref="System.Text.Json"/>. </summary>
    /// <remarks>
    /// The <see cref="Context"/> property carries an opaque caller-supplied object that specific factory layers can use to thread
    /// contextual callbacks (for example, an entity reconstructor callback for container components) without coupling this
    /// generic type to any domain.  The generic prefab infrastructure never reads or writes <see cref="Context"/>.
    /// </remarks>
    public sealed class PrefabData
    {
        /// <summary> Snake_case-keyed dictionary of cloned JSON values, built once at construction from the source JSON object's properties. </summary>
        private readonly Dictionary<String, JsonElement> _properties;

        /// <summary> The part type key, recorded here for use in error messages. </summary>
        private readonly String _typeKey;

        /// <summary> An opaque caller-supplied context object; set by the entity layer to carry domain-specific callbacks into factory methods. </summary>
        /// <remarks> This property is never read by the generic prefab infrastructure. </remarks>
        public Object? Context { get; internal set; }


        /// <summary> Initialises a new wrapper over the supplied JSON element, immediately building the normalised property dictionary and cloning each value for lifetime safety. </summary>
        /// <param name="element"> The JSON object element representing one part entry; its properties are iterated once and cloned. The <c>"type"</c> discriminator field written by <see cref="PrefabLoader{TOwner,TPart}"/> becomes a queryable normalised property and participates in the ambiguity guard like any other property. </param>
        /// <param name="typeKey"> The part type identifier, used to enrich error messages. </param>
        /// <exception cref="InvalidOperationException"> Thrown when two properties in <paramref name="element"/> normalise to the same snake_case key; the message names both original property names. </exception>
        internal PrefabData(JsonElement element, String typeKey)
        {
            _typeKey    = typeKey;
            _properties = BuildNormalisedDictionary(element, typeKey);
        }


        /// <summary> Returns the string value for <paramref name="key"/>, or throws if absent or not a string. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <returns> The property's string value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the normalised key is not present. The message names both the original key and its normalised form. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.String"/>. </exception>
        public String GetString(String key)
        {
            return Get(key, k => k == JsonValueKind.String, "String", e => e.GetString()!);
        }


        /// <summary> Returns the 32-bit integer value for <paramref name="key"/>, or throws if absent or not a number. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <returns> The property's integer value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the normalised key is not present. The message names both the original key and its normalised form. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property value kind is not <see cref="JsonValueKind.Number"/> or cannot be represented as <see cref="Int32"/>. </exception>
        public Int32 GetInt32(String key)
        {
            return Get(key, k => k == JsonValueKind.Number, "Number", e =>
            {
                Boolean parsed = e.TryGetInt32(out Int32 value);

                if (!parsed)
                {
                    throw new InvalidOperationException($"Prefab part '{_typeKey}': property '{key}' cannot be represented as Int32.");
                }

                return value;
            });
        }


        /// <summary> Returns the single-precision floating-point value for <paramref name="key"/>, or throws if absent or not a number. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <returns> The property's single value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the normalised key is not present. The message names both the original key and its normalised form. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property value kind is not <see cref="JsonValueKind.Number"/> or cannot be represented as <see cref="Single"/>. </exception>
        public Single GetSingle(String key)
        {
            return Get(key, k => k == JsonValueKind.Number, "Number", e =>
            {
                Boolean parsed = e.TryGetSingle(out Single value);

                if (!parsed)
                {
                    throw new InvalidOperationException($"Prefab part '{_typeKey}': property '{key}' cannot be represented as Single.");
                }

                return value;
            });
        }


        /// <summary> Returns the boolean value for <paramref name="key"/>, or throws if absent or not a boolean. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <returns> The property's boolean value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the normalised key is not present. The message names both the original key and its normalised form. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is neither <see cref="JsonValueKind.True"/> nor <see cref="JsonValueKind.False"/>. </exception>
        public Boolean GetBoolean(String key)
        {
            return Get(key, k => k == JsonValueKind.True || k == JsonValueKind.False, "Boolean", e => e.GetBoolean());
        }


        /// <summary> Attempts to retrieve the string value for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <param name="value"> When this method returns <c>true</c>, contains the property's string value; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the normalised key exists and is a string; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.String"/>. </exception>
        public Boolean TryGetString(String key, out String? value)
        {
            return TryGet(key, k => k == JsonValueKind.String, "String", e => e.GetString()!, out value);
        }


        /// <summary> Attempts to retrieve the 32-bit integer value for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <param name="value"> When this method returns <c>true</c>, contains the property's integer value; otherwise the default. </param>
        /// <returns> <c>true</c> if the normalised key exists and is a representable integer; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.Number"/>, or when the number cannot be represented as <see cref="Int32"/>. </exception>
        public Boolean TryGetInt32(String key, out Int32 value)
        {
            return TryGet(key, k => k == JsonValueKind.Number, "Number", e =>
            {
                Boolean parsed = e.TryGetInt32(out Int32 inner);

                if (!parsed)
                {
                    throw new InvalidOperationException($"Prefab part '{_typeKey}': property '{key}' cannot be represented as Int32.");
                }

                return inner;
            }, out value);
        }


        /// <summary> Attempts to retrieve the single-precision floating-point value for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <param name="value"> When this method returns <c>true</c>, contains the property's single value; otherwise the default. </param>
        /// <returns> <c>true</c> if the normalised key exists and is a representable single; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.Number"/>, or when the number cannot be represented as <see cref="Single"/>. </exception>
        public Boolean TryGetSingle(String key, out Single value)
        {
            return TryGet(key, k => k == JsonValueKind.Number, "Number", e =>
            {
                Boolean parsed = e.TryGetSingle(out Single inner);

                if (!parsed)
                {
                    throw new InvalidOperationException($"Prefab part '{_typeKey}': property '{key}' cannot be represented as Single.");
                }

                return inner;
            }, out value);
        }


        /// <summary> Attempts to retrieve the boolean value for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <param name="value"> When this method returns <c>true</c>, contains the property's boolean value; otherwise the default. </param>
        /// <returns> <c>true</c> if the normalised key exists and is a boolean; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is neither <see cref="JsonValueKind.True"/> nor <see cref="JsonValueKind.False"/>. </exception>
        public Boolean TryGetBoolean(String key, out Boolean value)
        {
            return TryGet(key, k => k == JsonValueKind.True || k == JsonValueKind.False, "Boolean", e => e.GetBoolean(), out value);
        }


        /// <summary> Returns the string-array value for <paramref name="key"/>, or throws if absent, not a JSON array, or any element is not a string. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <returns> A read-only list of the string elements in the array, in document order. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the normalised key is not present; the message names both the original key and its normalised form. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but is not a JSON array, or when any element within the array is not a string; the message names the key and the offending element kind. </exception>
        public IReadOnlyList<String> GetStringArray(String key)
        {
            return Get(key, k => k == JsonValueKind.Array, "Array", e => ExtractStringArray(key, e));
        }


        /// <summary> Attempts to retrieve the string-array value for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <param name="value"> When this method returns <c>true</c>, contains the string list; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the normalised key exists and is a string array; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but is not a JSON array, or when any element is not a string; the message names the key and the offending element kind. </exception>
        public Boolean TryGetStringArray(String key, out IReadOnlyList<String>? value)
        {
            return TryGet(key, k => k == JsonValueKind.Array, "Array", e => (IReadOnlyList<String>)ExtractStringArray(key, e), out value);
        }


        /// <summary> Attempts to retrieve a JSON-object array for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <remarks> Each element is returned as a cloned <see cref="JsonElement"/> independent of the source document lifetime. Intended for use by container component factories that need to reconstruct nested entities from their saved JSON payloads. </remarks>
        /// <param name="key"> The property name to look up; normalised to snake_case before lookup. </param>
        /// <param name="value"> When this method returns <c>true</c>, a list of cloned <see cref="JsonElement"/> values representing each array element; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the normalised key exists and its value is a JSON array; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.Array"/>. </exception>
        public Boolean TryGetJsonArray(String key, out IReadOnlyList<JsonElement>? value)
        {
            return TryGet(key, k => k == JsonValueKind.Array, "Array", e => (IReadOnlyList<JsonElement>)ExtractJsonArray(e), out value);
        }


        /// <summary> Iterates a validated JSON array element and returns each child element as a cloned <see cref="JsonElement"/>, ensuring independence from the source document lifetime. </summary>
        /// <param name="arrayElement"> The JSON element whose kind has already been confirmed as <see cref="JsonValueKind.Array"/>. </param>
        /// <returns> An immutable list of cloned child elements in document order. </returns>
        private static List<JsonElement> ExtractJsonArray(JsonElement arrayElement)
        {
            List<JsonElement> result = new List<JsonElement>();

            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                result.Add(item.Clone());
            }

            return result;
        }


        /// <summary> Iterates a validated JSON array element and extracts each child as a string, throwing on the first non-string element. </summary>
        /// <param name="key"> The original caller-supplied key; used verbatim in the element-kind error message. </param>
        /// <param name="arrayElement"> The JSON element whose <see cref="JsonValueKind"/> has already been confirmed as <see cref="JsonValueKind.Array"/>. </param>
        /// <returns> An immutable list of string values in document order. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when any array element is not a <see cref="JsonValueKind.String"/>; the message names both the key and the offending element kind. </exception>
        private List<String> ExtractStringArray(String key, JsonElement arrayElement)
        {
            List<String> result = new List<String>();

            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                Boolean isString = item.ValueKind == JsonValueKind.String;

                if (!isString)
                {
                    throw new InvalidOperationException(
                        $"Prefab part '{_typeKey}': property '{key}' array contains a {item.ValueKind} element, but all elements must be strings.");
                }

                result.Add(item.GetString()!);
            }

            return result;
        }


        /// <summary> Shared throwing core for all <c>Get*</c> accessors: resolves the property (throwing <see cref="KeyNotFoundException"/> when absent), validates its kind, then applies <paramref name="read"/> to extract the typed value. </summary>
        /// <typeparam name="T"> The target value type. </typeparam>
        /// <param name="key"> The caller-supplied property name; passed through to <see cref="ResolveProperty"/> and <see cref="ValidateKind"/>. </param>
        /// <param name="isExpected"> Predicate that returns <c>true</c> for every acceptable <see cref="JsonValueKind"/>. </param>
        /// <param name="expectedLabel"> Human-readable type label used in the kind-mismatch error message. </param>
        /// <param name="read"> Delegate that extracts the typed value from the validated element; responsible for throwing <see cref="InvalidOperationException"/> when numeric precision is lost (Int32, Single). </param>
        /// <returns> The typed value extracted by <paramref name="read"/>. </returns>
        /// <exception cref="KeyNotFoundException"> Propagated from <see cref="ResolveProperty"/> when the key is absent. </exception>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="ValidateKind"/> on kind mismatch, or from <paramref name="read"/> on parse failure. </exception>
        private T Get<T>(String key, Func<JsonValueKind, Boolean> isExpected, String expectedLabel, Func<JsonElement, T> read)
        {
            JsonElement property = ResolveProperty(key);
            ValidateKind(key, property, isExpected, expectedLabel);
            return read(property);
        }


        /// <summary> Shared non-throwing core for all <c>TryGet*</c> accessors: attempts to find the property and, when present, validates its kind and applies <paramref name="read"/> to extract the typed value. </summary>
        /// <typeparam name="T"> The target value type. </typeparam>
        /// <param name="key"> The caller-supplied property name; passed through to <see cref="TryResolveProperty"/> and <see cref="ValidateKind"/>. </param>
        /// <param name="isExpected"> Predicate that returns <c>true</c> for every acceptable <see cref="JsonValueKind"/>. </param>
        /// <param name="expectedLabel"> Human-readable type label used in the kind-mismatch error message. </param>
        /// <param name="read"> Delegate that extracts the typed value from the validated element; responsible for throwing <see cref="InvalidOperationException"/> when numeric precision is lost (Int32, Single). </param>
        /// <param name="value"> When the method returns <c>true</c>, the extracted value; otherwise <c>default</c>. </param>
        /// <returns> <c>true</c> when the property is present; <c>false</c> when absent. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="ValidateKind"/> on kind mismatch, or from <paramref name="read"/> on parse failure. </exception>
        private Boolean TryGet<T>(String key, Func<JsonValueKind, Boolean> isExpected, String expectedLabel, Func<JsonElement, T> read, out T value)
        {
            Boolean found  = TryResolveProperty(key, out JsonElement property);
            Boolean result;

            if (!found)
            {
                value  = default!;
                result = false;
            }
            else
            {
                ValidateKind(key, property, isExpected, expectedLabel);
                value  = read(property);
                result = true;
            }

            return result;
        }


        /// <summary> Resolves the normalised key and throws <see cref="KeyNotFoundException"/> when absent; the throwing entry point for all <c>Get*</c> accessors. </summary>
        /// <param name="key"> The property name as supplied by the caller; normalised to snake_case before lookup. </param>
        /// <returns> The matching cloned <see cref="JsonElement"/>. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the normalised key is not present; the message names both the original key and its normalised form. </exception>
        private JsonElement ResolveProperty(String key)
        {
            Boolean found = TryResolveProperty(key, out JsonElement property);

            if (!found)
            {
                String normalisedKey = key.ToSnakeCase();
                throw new KeyNotFoundException($"Prefab part '{_typeKey}': required property '{key}' (normalised: '{normalisedKey}') was not found in the JSON data.");
            }

            return property;
        }


        /// <summary> Looks up the snake_case-normalised key in the property dictionary without throwing; the shared core used by both <c>Get*</c> and <c>TryGet*</c> paths. </summary>
        /// <param name="key"> The property name as supplied by the caller; normalised to snake_case before lookup. </param>
        /// <param name="property"> When this method returns <c>true</c>, contains the matching <see cref="JsonElement"/>; otherwise the default. </param>
        /// <returns> <c>true</c> when the normalised key exists in the property dictionary. </returns>
        private Boolean TryResolveProperty(String key, out JsonElement property)
        {
            String normalisedKey = key.ToSnakeCase();
            return _properties.TryGetValue(normalisedKey, out property);
        }


        /// <summary> Checks that <paramref name="property"/>'s <see cref="JsonValueKind"/> satisfies <paramref name="isExpected"/>, throwing an <see cref="InvalidOperationException"/> with the standard message when it does not. </summary>
        /// <param name="key"> The original caller-supplied key, used verbatim in the error message. </param>
        /// <param name="property"> The element whose kind is being validated. </param>
        /// <param name="isExpected"> Predicate returning <c>true</c> for every acceptable <see cref="JsonValueKind"/>; supports multi-kind checks such as boolean (True OR False). </param>
        /// <param name="expectedLabel"> The human-readable type label used in the error message (e.g. <c>"String"</c>, <c>"Number"</c>, <c>"Boolean"</c>). </param>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="isExpected"/> returns <c>false</c> for <paramref name="property"/>'s kind. </exception>
        private void ValidateKind(String key, JsonElement property, Func<JsonValueKind, Boolean> isExpected, String expectedLabel)
        {
            Boolean kindMatches = isExpected(property.ValueKind);

            if (!kindMatches)
            {
                throw new InvalidOperationException($"Prefab part '{_typeKey}': property '{key}' is {property.ValueKind}, expected {expectedLabel}.");
            }
        }


        /// <summary> Iterates the properties of <paramref name="element"/> once, normalising each property name to snake_case and cloning its value for lifetime safety. </summary>
        /// <param name="element"> The JSON object whose properties are to be indexed. </param>
        /// <param name="typeKey"> Used to enrich the ambiguity error message. </param>
        /// <returns> A dictionary mapping normalised snake_case keys to cloned <see cref="JsonElement"/> values. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when two properties normalise to the same key; the message names both original property names. </exception>
        private static Dictionary<String, JsonElement> BuildNormalisedDictionary(JsonElement element, String typeKey)
        {
            Dictionary<String, JsonElement> result       = new Dictionary<String, JsonElement>();
            Dictionary<String, String>      originalKeys = new Dictionary<String, String>();

            foreach (JsonProperty property in element.EnumerateObject())
            {
                String normalisedKey = property.Name.ToSnakeCase();
                Boolean collision    = originalKeys.TryGetValue(normalisedKey, out String? existingOriginal);

                if (collision)
                {
                    throw new InvalidOperationException($"Prefab part '{typeKey}': properties '{existingOriginal}' and '{property.Name}' both normalise to '{normalisedKey}'. Remove one to make the mapping unambiguous.");
                }

                result[normalisedKey]       = property.Value.Clone();
                originalKeys[normalisedKey] = property.Name;
            }

            return result;
        }
    }
}
