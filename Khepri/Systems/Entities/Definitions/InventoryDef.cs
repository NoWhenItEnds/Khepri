using System;
using System.Collections.Generic;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities.Definitions
{
    /// <summary> Authored definition of an <see cref="InventoryComponent"/>, including the entity prefabs the inventory is initially populated with. </summary>
    /// <remarks> Contents are direct <see cref="EntityPrefab"/> references, so a chest-containing-a-goblin is expressed by dragging the goblin resource into <see cref="Contents"/> in the Inspector; instantiation recurses naturally. </remarks>
    [GlobalClass]
    public partial class InventoryDef : ComponentDef
    {
        /// <summary> The entity prefabs instantiated and placed into the inventory when the owning entity is built. </summary>
        [Export] public Godot.Collections.Array<EntityPrefab> Contents { get; set; } = new();


        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException"> Thrown when a contained entity cannot be added (a duplicate instance or a runtime containment cycle), or when a content prefab transitively contains itself. </exception>
        public override Component Create(Entity owner, ISet<EntityPrefab> ancestry)
        {
            InventoryComponent inventory = new InventoryComponent(owner);

            foreach (EntityPrefab prefab in Contents)
            {
                Entity  child = prefab.Instantiate(ancestry);   // Cycle-aware: shares the recursion-stack set so a self-containing prefab fails fast.
                Boolean added = inventory.AddEntity(child);

                if (!added)
                {
                    throw new InvalidOperationException(
                        $"Could not add '{prefab.Name}' to the inventory on entity '{owner.UId}' (duplicate instance or containment cycle).");
                }
            }

            return inventory;
        }
    }
}
