using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Khepri.Entities.Components
{
    /// <summary> Decorates an entity's name with its visible condition — either derived from health or authored directly. </summary>
    [GlobalClass]
    public partial class ConditionComponent : AdjectiveComponent
    {
        /// <summary> The maximum stamina for this entity. </summary>
        [ExportGroup("Stamina")]
        [Export] public Single StaminaMaximum { get; set; } = 1f;

        /// <summary> The current stamina for this entity. </summary>
        [Export] public Single StaminaCurrent { get; set; } = 1f;

        /// <summary> Designer-authored stamina bands mapping a minimum fraction threshold to a condition adjective. </summary>
        [Export] public Godot.Collections.Dictionary<Single, String> DescriptionBands { get; set; } = new Godot.Collections.Dictionary<Single, String>()
        {
            { 0.00f, "Gravely Wounded" },
            { 0.25f, "Badly Wounded"   },
            { 0.50f, "Injured"         },
            { 0.75f, "Healthy"         },
            { 0.90f, "Vigorous"        }
        };


        /// <summary> The maximum willpower for this entity. </summary>
        [ExportGroup("Willpower")]
        [Export] public Single WillpowerMaximum { get; set; } = 1f;

        /// <summary> The current willpower for this entity. </summary>
        [Export] public Single WillpowerCurrent { get; set; } = 1f;


        /// <summary> The entity's stamina as a fraction of its maximum (0.0 = none, 1.0 = full). </summary>
        public Single StaminaPercentage => StaminaCurrent / StaminaMaximum;

        /// <summary> The entity's willpower as a fraction of its maximum (0.0 = none, 1.0 = full). </summary>
        public Single WillpowerPercentage => WillpowerCurrent / WillpowerMaximum;


        /// <summary> Set the entity's current stamina. </summary>
        /// <param name="stamina"> The new value to set the entity's stamina to. </param>
        public void SetCurrentStamina(Single stamina)
        {
            StaminaCurrent = Math.Clamp(stamina, 0, StaminaMaximum);
        }


        /// <summary> Set the entity's current willpower. </summary>
        /// <param name="willpower"> The new value to set the entity's willpower to. </param>
        public void SetCurrentWillpower(Single willpower)
        {
            WillpowerCurrent = Math.Clamp(willpower, 0, WillpowerMaximum);
        }


        /// <inheritdoc/>
        public override String GetAdjective()
        {
            String resolved = "unknown";
            IOrderedEnumerable<KeyValuePair<Single, String>> sortedDescriptions = DescriptionBands.OrderBy(pair => pair.Key);
            Single percentage = StaminaPercentage;

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
