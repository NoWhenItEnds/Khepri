using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Khepri.Entities.Definitions;

namespace Khepri.Entities.Components
{
    /// <summary> A component allowing an entity to hold other entities. </summary>
    /// <remarks> <see cref="Contents"/> authors the prefabs the inventory starts populated with; <see cref="OnInstantiate"/> instantiates each as a live child entity. The held entities are runtime-only state and are not exported. </remarks>
    [GlobalClass]
    public partial class InventoryComponent : Component, IEntityContainer
    {
        /// <summary> The entity prefabs this inventory is populated with at spawn time. Direct resource references, so a chest-containing-a-goblin is authored by dragging the goblin prefab into this list. </summary>
        [Export] public Godot.Collections.Array<EntityPrefab> Contents { get; set; } = new();


        /// <summary> The entities currently stored in this inventory; populated at spawn and not serialised by the field itself. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();


        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException"> Thrown when a contained entity cannot be added (a duplicate instance or a runtime containment cycle), or when a content prefab transitively contains itself. </exception>
        public override void OnInstantiate(ISet<EntityPrefab> ancestry)
        {
            foreach (EntityPrefab prefab in Contents)
            {
                Entity  child = prefab.Instantiate(ancestry);   // Cycle-aware: shares the recursion-stack set so a self-containing prefab fails fast.
                Boolean added = AddEntity(child);

                if (!added)
                {
                    throw new InvalidOperationException(
                        $"Could not add '{prefab.Name}' to the inventory on entity '{Owner.UId}' (duplicate instance or containment cycle).");
                }
            }
        }


        /// <inheritdoc/>
        public Boolean AddEntity(Entity entity)
        {
            Boolean wouldCycle = entity.Equals(Owner)
                || entity.GetContainers().Any(c => c.Contains(Owner));
            Boolean result = !wouldCycle && _entities.Add(entity);
            return result;
        }


        /// <inheritdoc/>
        public Boolean RemoveEntity(Entity entity) => _entities.Remove(entity);


        /// <inheritdoc/>
        public IReadOnlyCollection<Entity> GetEntities() => _entities.ToArray();
    }
}
