using System;
using System.Collections.Generic;
using Godot;
using Khepri.Entities.Definitions;

namespace Khepri.Entities.Components
{
    /// <summary> A component giving an entity the ability to be damaged. </summary>
    /// <remarks> <see cref="Min"/> and <see cref="Max"/> author an inclusive range; <see cref="OnInstantiate"/> rolls a concrete maximum within it and collapses the range to that value, so the stored state is stable across saves and re-instantiation. </remarks>
    [GlobalClass]
    public partial class HealthComponent : Component
    {
        /// <summary> The inclusive lower bound of the maximum-health roll. After instantiation this equals <see cref="Max"/>. </summary>
        [Export] public Int32 Min { get; set; } = 1;

        /// <summary> The authored inclusive upper bound; after instantiation, the rolled, effective maximum health. </summary>
        [Export] public Int32 Max { get; set; } = 1;


        /// <inheritdoc/>
        public override void OnInstantiate(ISet<EntityPrefab> ancestry)
        {
            Max = Random.Shared.Next(Min, Max + 1);   // TODO - Seed for determinism when save/replay is added.
            Min = Max;
            // TODO - Extend to have current health, defence, etc.
        }
    }
}
