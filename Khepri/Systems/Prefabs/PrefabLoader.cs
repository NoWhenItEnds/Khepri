using System;
using System.IO;
using System.Text.Json;

namespace Khepri.Prefabs
{
    /// <summary> Parses a JSON file that describes a prefab's part set and produces a <see cref="LoadedPrefab{TOwner,TPart}"/>, delegating actual part construction to a <see cref="FactoryRegistry{TOwner,TPart}"/>. </summary>
    /// <remarks>
    /// Expected JSON schema — <c>"components"</c> is the array property name supplied at construction time:
    /// <code>
    /// {
    ///   "name": "goblin",
    ///   "components": [
    ///     { "type": "Health",   "max": 30 },
    ///     { "type": "Position", "x": 0, "y": 0 }
    ///   ]
    /// }
    /// </code>
    /// <c>name</c> must be non-blank; the parts array must be present and non-empty; each entry must have a <c>type</c> string matching a key registered in the <see cref="FactoryRegistry{TOwner,TPart}"/>; all other properties are forwarded via <see cref="PrefabData"/>.
    /// </remarks>
    /// <typeparam name="TOwner"> The type of the object built from the prefab; must implement <see cref="IPartContainer{TPart}"/>. </typeparam>
    /// <typeparam name="TPart"> The base type of parts produced by the registered factories. </typeparam>
    public sealed class PrefabLoader<TOwner, TPart> where TOwner : IPartContainer<TPart>
    {
        /// <summary> The registry used to resolve part type keys to factories during parsing. </summary>
        private readonly FactoryRegistry<TOwner, TPart> _registry;

        /// <summary> The JSON array property name that lists the parts for this prefab (e.g. <c>"components"</c>). </summary>
        private readonly String _partsPropertyName;


        /// <summary> Initialises the loader with the registry that maps type-keys to part factories, and the name of the JSON array property that lists the parts. </summary>
        /// <param name="registry"> The registry consulted for each part type encountered in JSON. </param>
        /// <param name="partsPropertyName"> The JSON property name of the parts array (e.g. <c>"components"</c> or <c>"features"</c>). Must not be null or whitespace. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="partsPropertyName"/> is null or whitespace. </exception>
        public PrefabLoader(FactoryRegistry<TOwner, TPart> registry, String partsPropertyName)
        {
            if (String.IsNullOrWhiteSpace(partsPropertyName))
            {
                throw new ArgumentException("Parts property name must not be null or whitespace.", nameof(partsPropertyName));
            }

            _registry          = registry;
            _partsPropertyName = partsPropertyName;
        }


        /// <summary> Parses the JSON content at <paramref name="filePath"/> and returns a <see cref="LoadedPrefab{TOwner,TPart}"/> whose <see cref="LoadedPrefab{TOwner,TPart}.Name"/> matches the file's <c>name</c> field. </summary>
        /// <param name="filePath"> The absolute or relative path to the prefab JSON file. </param>
        /// <returns> A <see cref="LoadedPrefab{TOwner,TPart}"/> containing the name and fully assembled prefab. </returns>
        /// <exception cref="IOException"> Thrown when the file cannot be read (includes <see cref="FileNotFoundException"/> and <see cref="UnauthorizedAccessException"/>), wrapped with <paramref name="filePath"/> as context. </exception>
        /// <exception cref="JsonException"> Thrown when the file content is not valid JSON, wrapped with <paramref name="filePath"/> as context. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the document is missing a non-blank <c>name</c>, when the parts array is absent or empty, when a part entry is missing the <c>type</c> field, or when an entry references a type key that has not been registered. </exception>
        public LoadedPrefab<TOwner, TPart> Load(String filePath)
        {
            String json = ReadFile(filePath);
            JsonDocument document = ParseJson(json, filePath);

            using (document)
            {
                JsonElement                root   = document.RootElement;
                String                     name   = ReadName(root, filePath);
                Prefab<TOwner, TPart>      prefab = BuildPrefab(root, filePath);

                return new LoadedPrefab<TOwner, TPart>(name, prefab);
            }
        }


        /// <summary> Reads the raw text content of the file, wrapping any IO or permission failure with <paramref name="filePath"/> as context. </summary>
        /// <param name="filePath"> The path to read. </param>
        /// <returns> The file's text content. </returns>
        /// <exception cref="IOException"> Thrown when the file cannot be read; <see cref="UnauthorizedAccessException"/> is also caught and wrapped so the file path is always present in the message. </exception>
        private static String ReadFile(String filePath)
        {
            String content;

            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new IOException($"Prefab file '{filePath}' could not be read: {ex.Message}", ex);
            }

            return content;
        }


        /// <summary> Parses the JSON string, wrapping any <see cref="JsonException"/> with the file path so callers can identify which file caused the failure. </summary>
        /// <param name="json"> The raw JSON text. </param>
        /// <param name="filePath"> The originating file path, used only in error context. </param>
        /// <returns> The parsed <see cref="JsonDocument"/>. </returns>
        /// <exception cref="JsonException"> Thrown with <paramref name="filePath"/> as context when parsing fails. </exception>
        private static JsonDocument ParseJson(String json, String filePath)
        {
            JsonDocument? document;

            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Prefab file '{filePath}' contains invalid JSON: {ex.Message}", ex);
            }

            return document;
        }


        /// <summary> Extracts and validates the <c>name</c> field from the root JSON element. </summary>
        /// <param name="root"> The root element of the prefab document. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> The non-blank prefab name. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>name</c> is absent, null, or whitespace. </exception>
        private static String ReadName(JsonElement root, String filePath)
        {
            Boolean hasName = root.TryGetProperty("name", out JsonElement nameElement);
            String? name    = hasName ? nameElement.GetString() : null;
            Boolean valid   = !String.IsNullOrWhiteSpace(name);

            if (!valid)
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': the 'name' field is missing or blank.");
            }

            return name!;
        }


        /// <summary> Builds a <see cref="Prefab{TOwner,TPart}"/> from the parts array of the root element, recording one factory closure per entry. </summary>
        /// <param name="root"> The root element of the prefab document. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> A fully populated prefab with one recorded factory per part entry. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the parts array is absent or empty, or when an entry lacks a non-blank <c>type</c> field, or when an entry's type is not registered in the registry. </exception>
        private Prefab<TOwner, TPart> BuildPrefab(JsonElement root, String filePath)
        {
            Boolean hasPartsProperty = root.TryGetProperty(_partsPropertyName, out JsonElement partsElement);

            if (!hasPartsProperty || partsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': the '{_partsPropertyName}' array is missing.");
            }

            if (partsElement.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': the '{_partsPropertyName}' array is empty. A prefab must declare at least one part.");
            }

            Prefab<TOwner, TPart> prefab = Prefab<TOwner, TPart>.Define();

            foreach (JsonElement entry in partsElement.EnumerateArray())
            {
                prefab = RecordPart(prefab, entry, filePath);
            }

            return prefab;
        }


        /// <summary> Reads one part entry, validates its <c>type</c> field, resolves the factory delegate from the registry at load time, and records a closure on the prefab that invokes it at owner-creation time. </summary>
        /// <remarks> Any <see cref="InvalidOperationException"/> from <see cref="FactoryRegistry{TOwner,TPart}.Resolve"/> is re-thrown with <paramref name="filePath"/> prepended so the offending file is always identifiable. </remarks>
        /// <param name="prefab"> The prefab being assembled; entries are chained onto it. </param>
        /// <param name="entry"> The JSON object for this part entry. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> The prefab with the new factory appended. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>type</c> is absent or blank; or when the type key is not registered, wrapped with <paramref name="filePath"/> as context so the offending file is identifiable during a multi-file directory load. </exception>
        private Prefab<TOwner, TPart> RecordPart(Prefab<TOwner, TPart> prefab, JsonElement entry, String filePath)
        {
            Boolean hasType = entry.TryGetProperty("type", out JsonElement typeElement);
            String? typeKey = hasType ? typeElement.GetString() : null;

            if (String.IsNullOrWhiteSpace(typeKey))
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': a part entry is missing the 'type' field.");
            }

            // Resolve the factory delegate once at load time — any unregistered type is caught here,
            // not later at owner-creation time (which could be much later and harder to diagnose).
            // The InvalidOperationException from Resolve is re-wrapped with the file path so a
            // maintainer loading many files at once can identify which file referenced the bad type.
            Func<TOwner, PrefabData, TPart> factory;

            try
            {
                factory = _registry.Resolve(typeKey);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    $"Prefab file '{filePath}': part type '{typeKey}' has not been registered in the FactoryRegistry.",
                    ex);
            }

            // Snapshot the element so the closure does not capture a JsonElement struct that could
            // be invalidated once the JsonDocument is disposed.
            JsonElement snapshot = entry.Clone();
            PrefabData  data     = new PrefabData(snapshot, typeKey);

            return prefab.WithPart(owner => factory(owner, data));
        }
    }
}
