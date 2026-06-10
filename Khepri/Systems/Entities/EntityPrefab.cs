using System;
using System.Collections.Generic;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> A reusable, designer-authored template describing the component set of an entity, saved as a <c>.tres</c> resource and instantiated into a live <see cref="Entity"/> on demand. </summary>
    [GlobalClass]
    public partial class EntityPrefab : Resource
    {
        /// <summary> The name this prefab is registered and looked up under (for example <c>"goblin"</c>). Must be unique across all loaded prefabs. </summary>
        [Export] public String Name { get; set; } = String.Empty;

        /// <summary> The template components applied to each entity built from this prefab. Each is duplicated per entity at instantiation. </summary>
        [Export] public Godot.Collections.Array<Component> Components { get; set; } = new();


        /// <summary> Builds a fresh <see cref="Entity"/> with a new identity and a private copy of each template component. </summary>
        /// <returns> A fully constructed entity. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when two templates resolve to the same component type, or when a container causes this prefab to transitively contain itself. </exception>
        public Entity Instantiate()
        {
            return Instantiate(new HashSet<EntityPrefab>());
        }


        /// <summary> Cycle-aware recursive core of <see cref="Instantiate()"/>: builds the entity while guarding against a prefab that contains itself through a container component. </summary>
        /// <remarks> <see cref="ResourceLoader"/> caches each <c>.tres</c> to a single instance, so reference identity in <paramref name="ancestry"/> reliably detects a revisited prefab. A prefab shared by two siblings (a diamond) is permitted — only prefabs currently open on the recursion stack are rejected. </remarks>
        /// <param name="ancestry"> The prefabs currently being instantiated higher up the recursion stack. </param>
        /// <returns> A fully constructed entity. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when this prefab is already on the recursion stack (a containment cycle), or when two templates resolve to the same component type. </exception>
        internal Entity Instantiate(ISet<EntityPrefab> ancestry)
        {
            Boolean fresh = ancestry.Add(this);

            if (!fresh)
            {
                throw new InvalidOperationException(
                    $"Prefab containment cycle detected: entity prefab '{Name}' (transitively) contains itself.");
            }

            Entity entity = new Entity(Guid.NewGuid());

            foreach (Component template in Components)
            {
                Component instance = (Component)template.Duplicate(false);   // Per-entity copy; share any referenced resources rather than deep-cloning prefab references.
                instance.Initialise(entity);
                instance.OnInstantiate(ancestry);

                Boolean added = entity.AddComponent(instance);

                if (!added)
                {
                    throw new InvalidOperationException(
                        $"Entity prefab '{Name}' declares two components that resolve to the same type ('{instance.GetType().Name}').");
                }
            }

            ancestry.Remove(this);

            return entity;
        }
    }
}
