using System;
using Khepri.Entities.Prefabs;
using Khepri.Prefabs;

namespace Khepri.Entities.Components
{
    /// <summary> A component giving an entity the ability to be damaged. </summary>
    public class HealthComponent : Component, IStatefulPart
    {
        /// <summary> The maximum amount of health this entity can have; rolled once at spawn and preserved across save/load cycles. </summary>
        public Int32 Max { get; }


        /// <summary> Initialises a new <see cref="HealthComponent"/> with a concrete maximum health value. </summary>
        /// <param name="entity"> The parent entity. </param>
        /// <param name="max"> The concrete maximum health value — the rolled result when spawning, or the saved value when restoring. </param>
        public HealthComponent(Entity entity, Int32 max) : base(entity)
        {
            Max = max;
            // TODO - Extend to have current health, defence, etc.
        }


        /// <summary> Creates or restores a <see cref="HealthComponent"/> from the supplied data, rolling a value when prefab range keys are present and reading the concrete saved value otherwise. </summary>
        /// <remarks>
        /// Spawn path (prefab data contains <c>"min"</c>): rolls a discrete value in the range [<c>"min"</c>, <c>"max"</c>] inclusive using <see cref="Random.Shared"/>.
        /// Restore path (save data contains only <c>"max"</c>): reads the concrete saved value directly without re-rolling.
        /// Both paths converge on the same constructor — the distinction is fully encapsulated here.
        /// Uses <see cref="Random.Shared"/> for simplicity; seeding and determinism are a future concern.
        /// </remarks>
        /// <param name="entity"> The entity the component will be attached to. </param>
        /// <param name="data"> The component's parsed data; must contain either both <c>"min"</c> and <c>"max"</c> (prefab) or just <c>"max"</c> (save). </param>
        /// <returns> A fully constructed <see cref="HealthComponent"/> with either a rolled or restored max value. </returns>
        [ComponentFactory]
        private static HealthComponent Create(Entity entity, PrefabData data)
        {
            Boolean isSpawn = data.TryGetInt32("min", out Int32 min);

            Int32 max = isSpawn
                ? Random.Shared.Next(min, data.GetInt32("max") + 1)
                : data.GetInt32("max");

            return new HealthComponent(entity, max);
        }


        /// <inheritdoc/>
        public void WriteState(StateWriter writer)
        {
            writer.SetInt32("max", Max);
        }
    }
}
