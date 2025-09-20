using System;
using System.Text.Json.Serialization;
using System.Threading;
using Godot;

namespace Khepri.Models.Persistent
{
    /// <summary> The persistent stats of a unit. These are saved and loaded to keep track of the world. </summary>
    public record UnitData : IPersistent
    {
        /// <inheritdoc/>
        [JsonPropertyName("uid")]
        public Guid UId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        [JsonPropertyName("position")]
        public Vector3 WorldPosition { get; private set; } = Vector3.Zero;   // TODO - Figure out what to do here.

        /// <summary> The unit's base movement speed. </summary>
        [JsonPropertyName("base_speed")]
        public Single BaseSpeed { get; private set; } = 3f;

        /// <summary> The amount to modify the base speed amount for sprinting. </summary>
        [JsonPropertyName("sprint_modifier")]
        public Single SprintModifier { get; private set; } = 2f;

        /// <summary> A value between 0-100 representing the unit's current physical state. </summary>
        [JsonPropertyName("health")]
        public Single CurrentHealth { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current hunger. </summary>
        [JsonPropertyName("hunger")]
        public Single CurrentHunger { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current fatigue / tiredness. </summary>
        [JsonPropertyName("stamina")]
        public Single CurrentStamina { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current entertainment / boredom. </summary>
        [JsonPropertyName("entertainment")]
        public Single CurrentEntertainment { get; private set; } = 100f;

        /// <summary> The data structure representing the unit's inventory. </summary>
        [JsonPropertyName("inventory")]
        public EntityInventory Inventory { get; private set; } = new EntityInventory();


        /// <inheritdoc/>
        public void SaveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        /// <summary> Updates the unit's health by the given amount. </summary>
        /// <param name="amount"> The relative amount to alter the unit's health by. </param>
        public void UpdateHealth(Single amount)
        {
            CurrentHealth = Math.Clamp(CurrentHealth + amount, 0f, 100f);
            // TODO - Call some death handler when the unit is at zero.
        }


        /// <inheritdoc/>
        public Int32 CompareTo(IPersistent other) => UId.CompareTo(other.UId);
    }
}
