using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Godot;
using Khepri.Controllers;
using Khepri.Entities.Items;

namespace Khepri.Data.Actors
{
    /// <summary> The data component for a living creature within the game world. </summary>
    public class BeingData : ActorData
    {
        /// <summary> The unit's base movement speed. </summary>
        [JsonPropertyName("base_speed"), Required]
        public required Single BaseSpeed { get; init; } = 3f;

        /// <summary> The grid size of the being's inventory. </summary>
        [JsonPropertyName("inventory_size"), Required]
        public required Vector2I InventorySize { get; init; } = new Vector2I(10, 10);


        /// <summary> Modifies the amount of hunger the unit looses each tick. </summary>
        [JsonPropertyName("hunger_modifier"), Required]
        public required Single HungerModifier { get; init; } = 0.001f;

        /// <summary> Modifies the amount of fatigue the unit looses each tick. </summary>
        [JsonPropertyName("fatigue_modifier"), Required]
        public required Single FatigueModifier { get; init; } = 0.001f;

        /// <summary> Modifies the amount of entertainment the unit looses each tick. </summary>
        [JsonPropertyName("entertainment_modifier"), Required]
        public required Single EntertainmentModifier { get; init; } = 0.001f;

        /// <summary> The amount of stamina the unit recovers per tick. </summary>
        [JsonPropertyName("stamina_modifier"), Required]
        public required Single StaminaModifier { get; init; } = 1f;

        /// <summary> The amount to modify the base speed amount for sprinting. </summary>
        [JsonPropertyName("stamina_modifier"), Required]
        public required Single SprintModifier { get; init; } = 2f;


        /// <summary> A value between 0-100 representing the unit's current physical state. </summary>
        [JsonPropertyName("current_health")]
        public Single CurrentHealth { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current hunger. </summary>
        [JsonPropertyName("current_hunger")]
        public Single CurrentHunger { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current fatigue / tiredness. </summary>
        [JsonPropertyName("current_fatigue")]
        public Single CurrentFatigue { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current entertainment / boredom. </summary>
        [JsonPropertyName("current_entertainment")]
        public Single CurrentEntertainment { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current stamina. </summary>
        /// <remarks> This is modified by all the other stats (health, hunger, fatigue, etc.). </remarks>
        [JsonPropertyName("current_stamina")]
        public Single CurrentStamina { get; private set; } = 100f;


        /// <summary> A reference to the actor's inventory component. </summary>
        public EntityInventory Inventory { get; private set; }

        /// <summary> A reference to the actor's equipment component. </summary>
        public EntityEquipment Equipment { get; private set; }


        /// <summary> The data component for a living creature within the game world. </summary>
        public BeingData() : base()
        {
            Inventory = new EntityInventory(InventorySize);
            Equipment = new EntityEquipment();
        }


        /// <summary> Update the needs internal state every tick. </summary>
        public void Update()
        {
            Single gameTimeDelta = (Single)WorldController.Instance.GameTimeDelta;
            UpdateHunger(-gameTimeDelta * HungerModifier);
            UpdateFatigue(-gameTimeDelta * FatigueModifier);
            UpdateEntertainment(-gameTimeDelta * EntertainmentModifier);
            UpdateStamina(gameTimeDelta * StaminaModifier);
        }


        /// <summary> Updates the unit's health by the given amount. </summary>
        /// <param name="amount"> The relative amount to alter the unit's health by. </param>
        public void UpdateHealth(Single amount)
        {
            CurrentHealth = Math.Clamp(CurrentHealth + amount, 0f, 100f);
            // TODO - Call some death handler when the unit is at zero.
        }


        /// <summary> Updates the unit's hunger by the given amount. </summary>
        /// <param name="amount"> The relative amount to alter the unit's hunger by. </param>
        public void UpdateHunger(Single amount)
        {
            CurrentHunger = Math.Clamp(CurrentHunger + amount, 0f, 100f);
            // TODO - Call some death handler when the unit is at zero.
        }


        /// <summary> Updates the unit's fatigue by the given amount. </summary>
        /// <param name="amount"> The relative amount to alter the unit's fatigue by. </param>
        public void UpdateFatigue(Single amount)
        {
            CurrentFatigue = Math.Clamp(CurrentFatigue + amount, 0f, 100f);
            // TODO - Call some death handler when the unit is at zero.
        }


        /// <summary> Updates the unit's entertainment by the given amount. </summary>
        /// <param name="amount"> The relative amount to alter the unit's entertainment by. </param>
        public void UpdateEntertainment(Single amount)
        {
            CurrentEntertainment = Math.Clamp(CurrentEntertainment + amount, 0f, 100f);
            // TODO - Call some death handler when the unit is at zero.
        }

        /// <summary> Updates the unit's entertainment by the given amount. </summary>
        /// <param name="amount"> The relative amount to alter the unit's entertainment by. </param>
        public void UpdateStamina(Single amount)
        {
            // Find the largest problem, and that caps the unit's stamina reserve.
            Single minValue = Math.Min(CurrentHealth, Math.Min(CurrentHunger, Math.Min(CurrentFatigue, CurrentEntertainment)));
            CurrentStamina = Math.Clamp(CurrentStamina + amount, 0f, minValue);
        }
    }
}
