using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Khepri.Descriptions;

namespace Khepri.Entities.Components
{
    /// <summary> Decorates an entity's name with its visible condition — either derived from health or authored directly. </summary>
    [GlobalClass]
    public partial class ConditionComponent : AdjectiveComponent
    {
        /// <summary> The maximum health for this entity. </summary>
        [Export] public Single Max { get; set; } = 1f;

        /// <summary> The current health for this entity. </summary>
        [Export] public Single Current { get; set; } = 1f;

        /// <summary> Designer-authored health bands mapping a minimum fraction threshold to a condition adjective. </summary>
        [Export] public Godot.Collections.Dictionary<Single, String> DescriptionBands { get; set; } = new Godot.Collections.Dictionary<Single, String>()
        {
            { 0.00f, "Gravely Wounded" },
            { 0.25f, "Badly Wounded"   },
            { 0.50f, "Injured"         },
            { 0.75f, "Healthy"         },
            { 0.90f, "Vigorous"        }
        };


        /// <summary> The entity's health as a fraction of its maximum (0.0 = none, 1.0 = full). </summary>
        public Single Percentage => Current / Max;

        /// <summary> Whether entity has health configured. </summary>
        public Boolean HasHealth => Current > 0f;


        /// <summary> Set the entity's current health. </summary>
        /// <param name="health"> The new value to set the entity's health to. </param>
        public void SetCurrentHealth(Single health)
        {
            Current = Math.Clamp(health, 0, Max);
        }


        /// <inheritdoc/>
        public override void Contribute(NameBuilder builder) => builder.Adjective(ResolveHealthWord());


        /// <summary> Resolves the condition word from <see cref="_sortedBands"/> for the current <see cref="_healthFraction"/>. </summary>
        /// <returns> The word from the winning band; never empty when bands are present. </returns>
        private String ResolveHealthWord()
        {
            String resolved = Word; // Default to the authored word when no health bands are configured.
            IOrderedEnumerable<KeyValuePair<Single, String>> sortedDescriptions = DescriptionBands.OrderBy(pair => pair.Key);
            Single percentage = Percentage;

            foreach (KeyValuePair<Single, String> band in sortedDescriptions)
            {
                if (band.Key <= percentage)
                {
                    resolved = band.Value;
                }
            }

            return resolved;
        }
    }
}
