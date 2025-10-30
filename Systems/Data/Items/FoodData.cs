using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Khepri.Data.Actors;
using Khepri.Entities.Actors;

namespace Khepri.Data.Items
{
    /// <summary> The data component of a food entity. </summary>
    public class FoodData : ItemData
    {
        /// <summary> The amount of a unit's health the item recovers. </summary>
        [JsonPropertyName("health_recovery"), Required]
        public required Single HealthRecovery { get; init; }

        /// <summary> The amount of a unit's hunger the item recovers. </summary>
        [JsonPropertyName("hunger_recovery"), Required]
        public required Single HungerRecovery { get; init; }

        /// <summary> The amount of a unit's fatigue the item recovers. </summary>
        [JsonPropertyName("fatigue_recovery"), Required]
        public required Single FatigueRecovery { get; init; }

        /// <summary> The amount of a unit's entertainment the item recovers. </summary>
        [JsonPropertyName("entertainment_recovery"), Required]
        public required Single EntertainmentRecovery { get; init; }

        /// <summary> The number of portions the item has at maximum. </summary>
        [JsonPropertyName("max_portions"), Required]
        public required Int32 MaxPortions { get; init; }


        /// <summary> The number of portions the item has remaining. </summary>
        [JsonPropertyName("portions")]
        public Int32 CurrentPortions { get; private set; }


        /// <summary> The data component of a food entity. </summary>
        public FoodData() : base()
        {
            CurrentPortions = MaxPortions;
        }


        /// <inheritdoc/>
        public override void Use(ActorNode activatingEntity)
        {
            BeingData resource = activatingEntity.GetData<BeingData>();
            resource.UpdateHealth(HealthRecovery);
            resource.UpdateHunger(HungerRecovery);
            resource.UpdateFatigue(FatigueRecovery);
            resource.UpdateEntertainment(EntertainmentRecovery);

            CurrentPortions -= 1;  // TODO - It should queue free.
        }
    }
}
