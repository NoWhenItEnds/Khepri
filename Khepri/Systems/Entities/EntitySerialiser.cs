using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Khepri.Entities.Components;
using Khepri.Prefabs;

namespace Khepri.Entities
{
    /// <summary> Converts a live <see cref="Entity"/> into a self-describing JSON save payload that contains every component and its current state. </summary>
    /// <remarks>
    /// The produced JSON has the shape:
    /// <code>
    /// { "uid": "&lt;guid&gt;", "components": [ { "type": "health", "max": 33 }, ... ] }
    /// </code>
    /// Every attached component is written — stateful or not — so that <see cref="EntityReconstructor"/> can rebuild the entity purely from the save without consulting any prefab.
    /// Components that implement <see cref="IStatefulPart"/> write their own state via <see cref="IStatefulPart.WriteState"/>; the serialiser passes a <see cref="StateWriter"/> whose <see cref="StateWriter.Context"/> carries a <c>Func&lt;Entity, JsonObject&gt;</c> callback so that container components (such as <see cref="Components.InventoryComponent"/>) can serialise their held entities under their own chosen key without the serialiser hardcoding any knowledge of nested-entity handling.
    /// The runtime containment invariant (a forest, not a graph — each entity is held by at most one container) means recursion always terminates; no additional visited-uid guard is applied.
    /// Serialisation fails explicitly when a component's runtime type has not been registered in the <see cref="TypeKeyMap"/>, indicating an authoring gap (a component class with no <c>[ComponentFactory]</c> method).
    /// Can be generalised to <c>TOwner</c>/<c>TPart</c> when room persistence is added; concrete to <see cref="Entity"/>/<see cref="Component"/> for now.
    /// </remarks>
    public sealed class EntitySerialiser
    {
        /// <summary> The reverse map used to look up a component's type-key from its runtime <see cref="Type"/>. </summary>
        private readonly TypeKeyMap _typeKeyMap;


        /// <summary> Initialises the serialiser with the type-key map built at startup by <see cref="ComponentDiscovery.RegisterAll"/>. </summary>
        /// <param name="typeKeyMap"> The map from component runtime type to JSON type-key; populated by <see cref="ComponentDiscovery.RegisterAll"/>. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="typeKeyMap"/> is null. </exception>
        public EntitySerialiser(TypeKeyMap typeKeyMap)
        {
            if (typeKeyMap is null)
            {
                throw new ArgumentNullException(nameof(typeKeyMap), "TypeKeyMap must not be null.");
            }

            _typeKeyMap = typeKeyMap;
        }


        /// <summary> Serialises <paramref name="entity"/> and all its components to a JSON string in the standard save format. </summary>
        /// <param name="entity"> The entity to serialise; must not be null. </param>
        /// <returns> A JSON string containing the entity's uid and component state array. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="entity"/> is null. </exception>
        /// <exception cref="KeyNotFoundException"> Propagated from <see cref="TypeKeyMap.Resolve"/> when a component's runtime type was never registered; indicates an authoring gap. </exception>
        public String Serialise(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity must not be null.");
            }

            JsonObject root = BuildEntityObject(entity);

            return root.ToJsonString();
        }


        /// <summary> Builds the root JSON object containing the entity's uid and its components array. </summary>
        /// <param name="entity"> The entity to represent. </param>
        /// <returns> The assembled root <see cref="JsonObject"/>. </returns>
        private JsonObject BuildEntityObject(Entity entity)
        {
            JsonArray  componentsArray = BuildComponentsArray(entity.GetComponents());
            JsonObject root            = new JsonObject
            {
                ["uid"]        = entity.UId.ToString(),
                ["components"] = componentsArray
            };

            return root;
        }


        /// <summary> Iterates all attached components and converts each to a JSON object entry in the array. </summary>
        /// <param name="components"> The component collection from the entity. </param>
        /// <returns> A <see cref="JsonArray"/> with one entry per component. </returns>
        private JsonArray BuildComponentsArray(IReadOnlyCollection<Component> components)
        {
            JsonArray array = new JsonArray();

            foreach (Component component in components)
            {
                JsonObject entry = BuildComponentEntry(component);
                array.Add(entry);
            }

            return array;
        }


        /// <summary> Serialises a single component: writes the <c>"type"</c> key and merges state properties when the component implements <see cref="IStatefulPart"/>. </summary>
        /// <remarks>
        /// When the component implements <see cref="IStatefulPart"/>, a <see cref="StateWriter"/> whose <see cref="StateWriter.Context"/> carries a <c>Func&lt;Entity, JsonObject&gt;</c> callback is passed to <see cref="IStatefulPart.WriteState"/>.
        /// Container components such as <see cref="Components.InventoryComponent"/> use this callback to write their held entities under a self-chosen key — the serialiser has no hardcoded knowledge of any nested-entity key.
        /// </remarks>
        /// <param name="component"> The component to serialise. </param>
        /// <returns> A JSON object with at least <c>"type"</c> set; stateful components also carry their self-written state properties. </returns>
        /// <exception cref="KeyNotFoundException"> Propagated from <see cref="TypeKeyMap.Resolve"/> when the component type is unregistered. </exception>
        private JsonObject BuildComponentEntry(Component component)
        {
            String     typeKey = _typeKeyMap.Resolve(component.GetType());
            JsonObject entry   = new JsonObject
            {
                ["type"] = typeKey
            };

            if (component is IStatefulPart stateful)
            {
                StateWriter writer = new StateWriter
                {
                    Context = (Func<Entity, JsonObject>)BuildEntityObject
                };

                stateful.WriteState(writer);

                JsonObject stateObject = writer.ToJsonObject();

                foreach (KeyValuePair<String, JsonNode?> property in stateObject)
                {
                    entry[property.Key] = property.Value?.DeepClone();
                }
            }

            return entry;
        }
    }
}
