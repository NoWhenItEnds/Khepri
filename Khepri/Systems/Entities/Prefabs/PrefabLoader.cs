using System;
using System.IO;
using System.Text.Json;
using Khepri.Entities.Components;

namespace Khepri.Entities.Prefabs
{
    /// <summary> Parses a JSON file that describes a prefab's component set and produces a <see cref="LoadedPrefab"/>, delegating actual component construction to a <see cref="ComponentRegistry"/>. </summary>
    /// <remarks>
    /// <para>
    /// The expected JSON schema for a prefab file is:
    /// <code>
    /// {
    ///   "name": "goblin",
    ///   "components": [
    ///     { "type": "Health",   "max": 30 },
    ///     { "type": "Position", "x": 0, "y": 0 }
    ///   ]
    /// }
    /// </code>
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>name</c> — required, non-blank; used as the catalogue key.</description></item>
    ///   <item><description><c>components</c> — required array; must contain at least one entry.</description></item>
    ///   <item><description>Each entry must have a <c>type</c> string matching a registered key in the <see cref="ComponentRegistry"/>.</description></item>
    ///   <item><description>All other properties on a component entry are forwarded to the factory via <see cref="ComponentData"/>.</description></item>
    /// </list>
    /// <para> This class is responsible for the JSON-to-Prefab conversion only. Directory scanning and catalogue management are handled by <see cref="PrefabCatalogue"/> (SRP). </para>
    /// </remarks>
    public sealed class PrefabLoader
    {
        /// <summary> The registry used to resolve component type keys to factories during parsing. </summary>
        private readonly ComponentRegistry _registry;


        /// <summary> Initialises the loader with the registry that maps type-keys to component factories. </summary>
        /// <param name="registry"> The registry consulted for each component type encountered in JSON. </param>
        public PrefabLoader(ComponentRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(registry);
            _registry = registry;
        }


        /// <summary> Parses the JSON content at <paramref name="filePath"/> and returns a <see cref="LoadedPrefab"/> whose <see cref="LoadedPrefab.Name"/> matches the file's <c>name</c> field. </summary>
        /// <param name="filePath"> The absolute or relative path to the prefab JSON file. </param>
        /// <returns> A <see cref="LoadedPrefab"/> containing the name and fully assembled prefab. </returns>
        /// <exception cref="IOException"> Thrown when the file cannot be read (includes <see cref="FileNotFoundException"/> and <see cref="UnauthorizedAccessException"/>), wrapped with <paramref name="filePath"/> as context. </exception>
        /// <exception cref="JsonException"> Thrown when the file content is not valid JSON, wrapped with <paramref name="filePath"/> as context. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the document is missing a non-blank <c>name</c>, when the <c>components</c> array is absent or empty, when a component entry is missing the <c>type</c> field, or when an entry references a type key that has not been registered. </exception>
        public LoadedPrefab Load(String filePath)
        {
            String json = ReadFile(filePath);
            JsonDocument document = ParseJson(json, filePath);

            using (document)
            {
                JsonElement       root      = document.RootElement;
                String            name      = ReadName(root, filePath);
                EntityPrefab      prefab    = BuildPrefab(root, filePath);

                return new LoadedPrefab(name, prefab);
            }
        }


        /// <summary> Reads the raw text content of the file, wrapping any IO or permission failure with <paramref name="filePath"/> as context so the caller can identify which file failed. </summary>
        /// <param name="filePath"> The path to read. </param>
        /// <returns> The file's text content. </returns>
        /// <exception cref="IOException"> Thrown with <paramref name="filePath"/> as context when the file cannot be read, including permission failures (<see cref="UnauthorizedAccessException"/> derives from <see cref="SystemException"/>, not <see cref="IOException"/>, so it is caught explicitly and wrapped here to ensure the file path is always present in the message). </exception>
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


        /// <summary> Builds a <see cref="EntityPrefab"/> from the <c>components</c> array of the root element, recording one factory closure per entry. </summary>
        /// <param name="root"> The root element of the prefab document. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> A fully populated prefab with one recorded factory per component entry. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>components</c> is absent or empty, or when an entry lacks a non-blank <c>type</c> field, or when an entry's type is not registered in the registry. </exception>
        private EntityPrefab BuildPrefab(JsonElement root, String filePath)
        {
            Boolean hasComponents = root.TryGetProperty("components", out JsonElement componentsElement);

            if (!hasComponents || componentsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': the 'components' array is missing.");
            }

            EntityPrefab prefab       = EntityPrefab.Define();
            Int32  entryCount   = 0;

            foreach (JsonElement entry in componentsElement.EnumerateArray())
            {
                prefab = RecordComponent(prefab, entry, filePath);
                entryCount++;
            }

            if (entryCount == 0)
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': the 'components' array is empty. A prefab must declare at least one component.");
            }

            return prefab;
        }


        /// <summary> Reads one component entry, validates its <c>type</c> field, and records a factory closure on the prefab that will call <see cref="ComponentRegistry.Create"/> at entity construction time. </summary>
        /// <param name="prefab"> The prefab being assembled; entries are chained onto it. </param>
        /// <param name="entry"> The JSON object for this component entry. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> The prefab with the new factory appended. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>type</c> is absent, blank, or refers to an unregistered component type. </exception>
        private EntityPrefab RecordComponent(EntityPrefab prefab, JsonElement entry, String filePath)
        {
            Boolean hasType = entry.TryGetProperty("type", out JsonElement typeElement);
            String? typeKey = hasType ? typeElement.GetString() : null;

            if (String.IsNullOrWhiteSpace(typeKey))
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': a component entry is missing the 'type' field.");
            }

            // Validate the type key eagerly so unknown types are caught at load time rather than
            // at entity construction time (which could be much later and harder to diagnose).
            Boolean typeRegistered = _registry.IsRegistered(typeKey);

            if (!typeRegistered)
            {
                throw new InvalidOperationException($"Prefab file '{filePath}': component type '{typeKey}' has not been registered in the ComponentRegistry.");
            }

            // Snapshot the element so the closure does not capture a JsonElement struct that could
            // be invalidated once the JsonDocument is disposed.
            JsonElement snapshot = entry.Clone();
            ComponentData data   = new ComponentData(snapshot, typeKey);

            return prefab.WithComponent<Component>(entity => _registry.Create(typeKey, entity, data));
        }
    }
}
