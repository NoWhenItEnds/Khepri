using System;
using System.Text.Json;
using Khepri.Entities.Components;
using Khepri.Entities.Prefabs;
using Khepri.Prefabs;

namespace Khepri.Entities
{
    /// <summary> Rebuilds an <see cref="Entity"/> from a self-describing save JSON payload without consulting any prefab or catalogue. </summary>
    /// <remarks>
    /// Expected JSON schema (produced by <see cref="EntitySerialiser"/>):
    /// <code>
    /// { "uid": "&lt;guid&gt;", "components": [ { "type": "health", "max": 33 }, ... ] }
    /// </code>
    /// Each component entry is resolved via the single factory registry populated by <see cref="ComponentDiscovery.RegisterAll"/>.
    /// Every factory is self-describing: a factory that handles both spawn and restore data (such as <see cref="HealthComponent"/> and <see cref="InventoryComponent"/>) inspects which keys are present to determine the correct path.
    /// To enable container components to reconstruct their held entities, the reconstructor places a <c>Func&lt;JsonElement, Entity&gt;</c> callback in <see cref="PrefabData.Context"/> before invoking each factory.
    /// Container components (such as <see cref="InventoryComponent"/>) use this callback to reconstruct and insert their children without the reconstructor needing any hardcoded knowledge of which key holds nested entities.
    /// The runtime containment invariant (a forest — each entity is held by at most one container) means this recursion always terminates; no additional visited-uid guard is applied.
    /// Can be generalised over <c>TOwner</c>/<c>TPart</c> when room persistence is added; concrete to <see cref="Entity"/>/<see cref="Component"/> for now.
    /// Parallels <see cref="PrefabCatalogue"/> beside <see cref="EntityFactory.CreateFrom"/>.
    /// </remarks>
    public sealed class EntityReconstructor
    {
        /// <summary> The single factory registry for all component types, used for both spawn and restore data. </summary>
        private readonly ComponentRegistry _registry;


        /// <summary> Initialises the reconstructor with the factory registry built by <see cref="ComponentDiscovery.RegisterAll"/>. </summary>
        /// <param name="registry"> The registry populated with <see cref="ComponentFactoryAttribute"/>-decorated methods; each factory is self-describing and handles both spawn and restore data. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="registry"/> is null. </exception>
        public EntityReconstructor(ComponentRegistry registry)
        {
            if (registry is null)
            {
                throw new ArgumentNullException(nameof(registry), "Component registry must not be null.");
            }

            _registry = registry;
        }


        /// <summary> Parses <paramref name="saveJson"/> and returns the fully reconstructed <see cref="Entity"/> with all components restored from saved state. </summary>
        /// <param name="saveJson"> The JSON string produced by <see cref="EntitySerialiser.Serialise"/>. </param>
        /// <returns> The reconstructed entity with identity and all components restored. </returns>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="saveJson"/> is null or whitespace. </exception>
        /// <exception cref="JsonException"> Thrown when <paramref name="saveJson"/> is not valid JSON. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the JSON is missing a valid <c>uid</c>, when the <c>components</c> array is absent, when an entry lacks a <c>type</c> field, or when a type-key is not registered. </exception>
        public Entity Reconstruct(String saveJson)
        {
            if (String.IsNullOrWhiteSpace(saveJson))
            {
                throw new ArgumentException("Save JSON must not be null or whitespace.", nameof(saveJson));
            }

            JsonDocument document = ParseJson(saveJson);

            Entity entity;

            using (document)
            {
                // Clone the root element so it survives the document disposal, then delegate to the shared recursive core.
                JsonElement rootSnapshot = document.RootElement.Clone();
                entity = ReconstructEntityFromElement(rootSnapshot);
            }

            return entity;
        }


        /// <summary> Parses the save JSON string, wrapping any <see cref="JsonException"/> with context. </summary>
        /// <param name="saveJson"> The raw JSON string. </param>
        /// <returns> The parsed <see cref="JsonDocument"/>. </returns>
        /// <exception cref="JsonException"> Thrown with contextual prefix when parsing fails. </exception>
        private static JsonDocument ParseJson(String saveJson)
        {
            JsonDocument? document;

            try
            {
                document = JsonDocument.Parse(saveJson);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Entity save data contains invalid JSON: {ex.Message}", ex);
            }

            return document;
        }


        /// <summary> Reads and validates the <c>uid</c> field from the root element. </summary>
        /// <param name="root"> The root JSON element. </param>
        /// <returns> The parsed <see cref="Guid"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>uid</c> is absent, blank, or not a valid GUID. </exception>
        private static Guid ReadUid(JsonElement root)
        {
            Boolean hasUid    = root.TryGetProperty("uid", out JsonElement uidElement);
            String? uidString = hasUid ? uidElement.GetString() : null;
            Boolean isBlank   = String.IsNullOrWhiteSpace(uidString);

            if (isBlank)
            {
                throw new InvalidOperationException("Entity save data: the 'uid' field is missing or blank.");
            }

            Boolean parsed = Guid.TryParse(uidString, out Guid uid);

            if (!parsed)
            {
                throw new InvalidOperationException($"Entity save data: 'uid' value '{uidString}' is not a valid GUID.");
            }

            return uid;
        }


        /// <summary> Reads the <c>components</c> array from the root element and applies each component entry to <paramref name="builder"/>. </summary>
        /// <param name="builder"> The entity builder to attach reconstructed components to. </param>
        /// <param name="root"> The root JSON element. </param>
        /// <returns> The builder with all components attached. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the <c>components</c> array is absent. </exception>
        private EntityBuilder ApplyComponents(EntityBuilder builder, JsonElement root)
        {
            Boolean hasComponents = root.TryGetProperty("components", out JsonElement componentsElement);
            Boolean isArray       = hasComponents && componentsElement.ValueKind == JsonValueKind.Array;

            if (!isArray)
            {
                throw new InvalidOperationException("Entity save data: the 'components' array is missing.");
            }

            EntityBuilder result = builder;

            foreach (JsonElement entry in componentsElement.EnumerateArray())
            {
                result = ApplyComponentEntry(result, entry);
            }

            return result;
        }


        /// <summary> Reads one component entry, resolves its factory from the single registry, and attaches the constructed component to the builder. </summary>
        /// <remarks>
        /// Before invoking the factory, the reconstructor sets <see cref="PrefabData.Context"/> to a <c>Func&lt;JsonElement, Entity&gt;</c> callback pointing at <see cref="ReconstructEntityFromElement"/>.
        /// Container components (such as <see cref="Components.InventoryComponent"/>) use this callback to reconstruct and insert their held entities under their own chosen key, with no hardcoded knowledge of nested-entity handling required from the reconstructor.
        /// </remarks>
        /// <param name="builder"> The entity builder to attach the component to. </param>
        /// <param name="entry"> The JSON object for this component entry. </param>
        /// <returns> The builder with the component attached. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the <c>type</c> field is absent or blank, or when the type-key is not registered. </exception>
        private EntityBuilder ApplyComponentEntry(EntityBuilder builder, JsonElement entry)
        {
            Boolean hasType = entry.TryGetProperty("type", out JsonElement typeElement);
            String? typeKey = hasType ? typeElement.GetString() : null;

            if (String.IsNullOrWhiteSpace(typeKey))
            {
                throw new InvalidOperationException("Entity save data: a component entry is missing the 'type' field.");
            }

            Boolean found = _registry.TryResolve(typeKey, out Func<Entity, PrefabData, Component>? factory);

            if (!found)
            {
                throw new InvalidOperationException(
                    $"Entity save data: component type '{typeKey}' is not registered in the factory registry. " +
                    $"Ensure a [ComponentFactory] method is declared for this type and that ComponentDiscovery.RegisterAll has been called.");
            }

            JsonElement snapshot = entry.Clone();
            PrefabData  data     = new PrefabData(snapshot, typeKey)
            {
                Context = (Func<JsonElement, Entity>)ReconstructEntityFromElement
            };

            builder.WithComponent(entity => factory!(entity, data));

            return builder;
        }


        /// <summary> Reconstructs a single entity from an already-parsed JSON element in the standard save schema (<c>{ "uid": ..., "components": [...] }</c>). </summary>
        /// <remarks>
        /// Shared recursive core used by both the top-level <see cref="Reconstruct"/> entry point and by container component factories via the <see cref="PrefabData.Context"/> callback.
        /// </remarks>
        /// <param name="root"> The root element of one entity save object. </param>
        /// <returns> The fully reconstructed <see cref="Entity"/> with all components and nested container contents restored. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="ReadUid"/> or <see cref="ApplyComponents"/> when the element is structurally invalid. </exception>
        private Entity ReconstructEntityFromElement(JsonElement root)
        {
            Guid          uid     = ReadUid(root);
            EntityBuilder builder = EntityFactory.Reconstruct(uid);

            builder = ApplyComponents(builder, root);

            return builder.Build();
        }
    }
}
