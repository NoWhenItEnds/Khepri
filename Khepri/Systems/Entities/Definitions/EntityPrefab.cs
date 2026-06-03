using System;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities.Definitions
{
    /// <summary> A reusable, designer-authored template describing the component set of an entity, saved as a <c>.tres</c> resource and instantiated into a live <see cref="Entity"/> on demand. </summary>
    /// <remarks>
    /// This replaces the former JSON-prefab + factory-registry stack: the editor owns the type identity (each <see cref="ComponentDef"/> is a <c>[GlobalClass]</c>), so there is no string-keyed type discriminator, no reflection scan, and no hand-written loader.
    /// Nested containers reference their contents as direct <see cref="EntityPrefab"/> resources (see <c>InventoryDef</c>), so population is plain recursion through <see cref="Instantiate"/> rather than a deferred name-resolution pass.
    /// </remarks>
    [GlobalClass]
    public partial class EntityPrefab : Resource
    {
        /// <summary> The name this prefab is registered and looked up under (for example <c>"goblin"</c>). Must be unique across all loaded prefabs. </summary>
        [Export] public String Name { get; set; } = String.Empty;

        /// <summary> The ordered component definitions applied to each entity built from this prefab. </summary>
        [Export] public Godot.Collections.Array<ComponentDef> Components { get; set; } = new();


        /// <summary> Builds a fresh <see cref="Entity"/> with a new identity and a component for each definition in <see cref="Components"/>. </summary>
        /// <returns> A fully constructed entity. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when two definitions produce components of the same type (the entity holds at most one component per type). </exception>
        public Entity Instantiate()
        {
            Entity entity = new Entity(Guid.NewGuid());

            foreach (ComponentDef definition in Components)
            {
                Component component = definition.Create(entity);
                Boolean   added     = entity.AddComponent(component);

                if (!added)
                {
                    throw new InvalidOperationException(
                        $"Entity prefab '{Name}' declares two components that resolve to the same type ('{component.GetType().Name}').");
                }
            }

            return entity;
        }
    }
}
