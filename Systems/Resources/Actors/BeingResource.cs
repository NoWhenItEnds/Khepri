using System;
using Godot;
using Khepri.Controllers;

namespace Khepri.Resources.Actors
{
    /// <summary> The data component for a living creature within the game world. </summary>
    [GlobalClass]
    public partial class BeingResource : ActorResource
    {
        /// <summary> Modifies the amount of hunger the unit looses each tick. </summary>
        [ExportGroup("Needs")]
        [ExportSubgroup("Modifiers")]
        [Export] public Single HungerModifier { get; private set; } = 0.001f;

        /// <summary> Modifies the amount of fatigue the unit looses each tick. </summary>
        [Export] public Single FatigueModifier { get; private set; } = 0.001f;

        /// <summary> Modifies the amount of entertainment the unit looses each tick. </summary>
        [Export] public Single EntertainmentModifier { get; private set; } = 0.001f;

        /// <summary> The amount of stamina the unit recovers per tick. </summary>
        [Export] public Single StaminaModifier { get; private set; } = 1f;

        /// <summary> The amount to modify the base speed amount for sprinting. </summary>
        [Export] public Single SprintModifier { get; private set; } = 2f;


        /// <summary> The unit's base movement speed. </summary>
        [ExportSubgroup("Current State")]
        [Export] public Single BaseSpeed { get; private set; } = 3f;

        /// <summary> A value between 0-100 representing the unit's current physical state. </summary>
        [Export] public Single CurrentHealth { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current hunger. </summary>
        [Export] public Single CurrentHunger { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current fatigue / tiredness. </summary>
        [Export] public Single CurrentFatigue { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current entertainment / boredom. </summary>
        [Export] public Single CurrentEntertainment { get; private set; } = 100f;

        /// <summary> A value between 0-100 representing the unit's current stamina. </summary>
        /// <remarks> This is modified by all the other stats (health, hunger, fatigue, etc.). </remarks>
        [Export] public Single CurrentStamina { get; private set; } = 100f;


        /// <summary> The data component for a living creature within the game world. </summary>
        public BeingResource() { }


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
