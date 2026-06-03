using System;
using System.Collections.Generic;

namespace Khepri.Entities
{
    /// <summary> Extends <see cref="IEntityContainer"/> with the notion of prefab-authored pending contents: the set of entity prefab names that an <see cref="EntityPopulator"/> should instantiate and insert after the entity is first built. </summary>
    /// <remarks>
    /// This seam exists to keep the factory layer Godot-free: the component factory records the desired contents as names (data), while population — which requires a live spawner — is deferred to <see cref="EntityPopulator"/>.
    /// Pending contents are a spawn-time concept only.  When reconstructing from a save, the component's own factory reads the serialised child-entity graph and reconstructs children directly, bypassing the pending-contents list entirely.
    /// </remarks>
    public interface IPopulatableContainer : IEntityContainer
    {
        /// <summary> Returns the prefab names this container was authored to be populated with, recorded at spawn time and not yet instantiated. </summary>
        /// <remarks> The list is consumed by <see cref="EntityPopulator.Populate"/> and is not meaningful after population completes. </remarks>
        /// <returns> A read-only collection of pending prefab name strings, in authoring order. </returns>
        public IReadOnlyCollection<String> GetPendingContents();
    }
}
