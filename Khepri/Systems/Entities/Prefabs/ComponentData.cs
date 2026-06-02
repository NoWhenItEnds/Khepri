using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Khepri.Entities.Prefabs
{
    /// <summary> A read-only payload wrapper over a single component's parsed JSON object, providing typed, fail-explicit accessors so that component factories never bind directly to <see cref="System.Text.Json"/>. </summary>
    /// <remarks> One instance per entry in the prefab's <c>components</c> array; accessors throw <see cref="KeyNotFoundException"/> for a missing key or <see cref="InvalidOperationException"/> for a type mismatch, naming the key in both cases. </remarks>
    public sealed class ComponentData
    {
        /// <summary> The raw JSON object backing this component's data. </summary>
        private readonly JsonElement _element;

        /// <summary> The component type key, recorded here for use in error messages. </summary>
        private readonly String _typeKey;


        /// <summary> Initialises a new wrapper over the supplied JSON element. </summary>
        /// <param name="element"> The JSON object element representing one component entry. </param>
        /// <param name="typeKey"> The component type identifier, used to enrich error messages. </param>
        internal ComponentData(JsonElement element, String typeKey)
        {
            _element = element;
            _typeKey  = typeKey;
        }


        /// <summary> Returns the string value for <paramref name="key"/>, or throws if absent or not a string. </summary>
        /// <param name="key"> The JSON property name to look up. </param>
        /// <returns> The property's string value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the JSON object does not contain a property named <paramref name="key"/>. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.String"/>. </exception>
        public String GetString(String key)
        {
            JsonElement property = GetProperty(key);
            Boolean isString = property.ValueKind == JsonValueKind.String;

            if (!isString)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' is {property.ValueKind}, expected String.");
            }

            return property.GetString()!;
        }


        /// <summary> Returns the 32-bit integer value for <paramref name="key"/>, or throws if absent or not a number. </summary>
        /// <param name="key"> The JSON property name to look up. </param>
        /// <returns> The property's integer value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the JSON object does not contain a property named <paramref name="key"/>. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property value kind is not <see cref="JsonValueKind.Number"/> or cannot be represented as <see cref="Int32"/>. </exception>
        public Int32 GetInt32(String key)
        {
            JsonElement property = GetProperty(key);
            Boolean isNumber = property.ValueKind == JsonValueKind.Number;

            if (!isNumber)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' is {property.ValueKind}, expected Number.");
            }

            Boolean parsed = property.TryGetInt32(out Int32 value);

            if (!parsed)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' cannot be represented as Int32.");
            }

            return value;
        }


        /// <summary> Returns the single-precision floating-point value for <paramref name="key"/>, or throws if absent or not a number. </summary>
        /// <param name="key"> The JSON property name to look up. </param>
        /// <returns> The property's single value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the JSON object does not contain a property named <paramref name="key"/>. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property value kind is not <see cref="JsonValueKind.Number"/> or cannot be represented as <see cref="Single"/>. </exception>
        public Single GetSingle(String key)
        {
            JsonElement property = GetProperty(key);
            Boolean isNumber = property.ValueKind == JsonValueKind.Number;

            if (!isNumber)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' is {property.ValueKind}, expected Number.");
            }

            Boolean parsed = property.TryGetSingle(out Single value);

            if (!parsed)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' cannot be represented as Single.");
            }

            return value;
        }


        /// <summary> Returns the boolean value for <paramref name="key"/>, or throws if absent or not a boolean. </summary>
        /// <param name="key"> The JSON property name to look up. </param>
        /// <returns> The property's boolean value. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the JSON object does not contain a property named <paramref name="key"/>. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is neither <see cref="JsonValueKind.True"/> nor <see cref="JsonValueKind.False"/>. </exception>
        public Boolean GetBoolean(String key)
        {
            JsonElement property = GetProperty(key);
            Boolean isBoolean = property.ValueKind == JsonValueKind.True
                             || property.ValueKind == JsonValueKind.False;

            if (!isBoolean)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' is {property.ValueKind}, expected Boolean.");
            }

            return property.GetBoolean();
        }


        /// <summary> Attempts to retrieve the string value for <paramref name="key"/> without throwing when the property is absent. </summary>
        /// <param name="key"> The JSON property name to look up. </param>
        /// <param name="value"> When this method returns <c>true</c>, contains the property's string value; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the property exists and is a string; <c>false</c> if absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the property exists but its value kind is not <see cref="JsonValueKind.String"/>. </exception>
        public Boolean TryGetString(String key, out String? value)
        {
            Boolean found = _element.TryGetProperty(key, out JsonElement property);
            Boolean result;

            if (!found)
            {
                value  = null;
                result = false;
            }
            else if (property.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException($"Component '{_typeKey}': property '{key}' is {property.ValueKind}, expected String.");
            }
            else
            {
                value  = property.GetString()!;
                result = true;
            }

            return result;
        }


        /// <summary> Retrieves the named JSON property, throwing a <see cref="KeyNotFoundException"/> when it is absent so callers receive a consistent, key-naming error. </summary>
        /// <param name="key"> The property name to resolve. </param>
        /// <returns> The matching <see cref="JsonElement"/>. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when the JSON object does not contain a property named <paramref name="key"/>. </exception>
        private JsonElement GetProperty(String key)
        {
            Boolean found = _element.TryGetProperty(key, out JsonElement property);

            if (!found)
            {
                throw new KeyNotFoundException($"Component '{_typeKey}': required property '{key}' was not found in the JSON data.");
            }

            return property;
        }
    }
}
