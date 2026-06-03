using System;
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
        /// <exception cref="InvalidOperationException"> Thrown when a contained entity cannot be added (a duplicate instance or a containment cycle). </exception>
        public override Component Create(Entity owner)
        {
            InventoryComponent inventory = new InventoryComponent(owner);

            foreach (EntityPrefab prefab in Contents)
            {
                Entity  child = prefab.Instantiate();   // TODO - Guard against cyclic prefab graphs if content authoring ever makes them likely.
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
