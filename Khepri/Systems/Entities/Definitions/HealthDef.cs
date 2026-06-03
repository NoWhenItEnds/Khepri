using System;
using System.Collections.Generic;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities.Definitions
{
    /// <summary> Authored definition of a <see cref="HealthComponent"/>: rolls a concrete maximum health in the inclusive range [<see cref="Min"/>, <see cref="Max"/>] at spawn time. </summary>
    [GlobalClass]
    public partial class HealthDef : ComponentDef
    {
        /// <summary> The inclusive lower bound of the rolled maximum-health range. </summary>
        [Export] public Int32 Min { get; set; } = 1;

        /// <summary> The inclusive upper bound of the rolled maximum-health range. </summary>
        [Export] public Int32 Max { get; set; } = 1;


        /// <inheritdoc/>
        public override Component Create(Entity owner, ISet<EntityPrefab> ancestry)
        {
            Int32 rolledMax = Random.Shared.Next(Min, Max + 1);   // TODO - Seed for determinism when save/replay is added.
            return new HealthComponent(owner, rolledMax);
        }
    }
}
