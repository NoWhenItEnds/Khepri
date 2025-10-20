using System;
using Godot;
using Godot.Collections;
using Khepri.Controllers;
using Khepri.Entities.Items;

namespace Khepri.Resources.Actors
{
    /// <summary> The data component for a living creature within the game world. </summary>
    [GlobalClass]
    public partial class BeingResource : ActorResource
    {
        /// <summary> The unit's base movement speed. </summary>
        [ExportGroup("Settings")]
        [Export] public Single BaseSpeed { get; private set; } = 3f;

        /// <summary> The grid size of the being's inventory. </summary>
        [Export] public Vector2I InventorySize { get; private set; } = new Vector2I(10, 10);


        /// <summary> Modifies the amount of hunger the unit looses each tick. </summary>
        [ExportSubgroup("Needs")]
        [Export] public Single HungerModifier { get; private set; } = 0.001f;

        /// <summary> Modifies the amount of fatigue the unit looses each tick. </summary>
        [Export] public Single FatigueModifier { get; private set; } = 0.001f;

        /// <summary> Modifies the amount of entertainment the unit looses each tick. </summary>
        [Export] public Single EntertainmentModifier { get; private set; } = 0.001f;

        /// <summary> The amount of stamina the unit recovers per tick. </summary>
        [Export] public Single StaminaModifier { get; private set; } = 1f;

        /// <summary> The amount to modify the base speed amount for sprinting. </summary>
        [Export] public Single SprintModifier { get; private set; } = 2f;


        /// <summary> A value between 0-100 representing the unit's current physical state. </summary>
        [ExportGroup("State")]
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


        /// <summary> A reference to the actor's inventory component. </summary>
        public EntityInventory Inventory { get; private set; }

        /// <summary> A reference to the actor's equipment component. </summary>
        public EntityEquipment Equipment { get; private set; }


        /// <summary> The data component for a living creature within the game world. </summary>
        public BeingResource()
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


        /// <inheritdoc/>
        public override Dictionary<String, Variant> Serialise()
        {
            Dictionary<Vector2I, Dictionary<String, Variant>> inventory = Inventory.Serialise();
            Dictionary<String, Dictionary<String, Variant>> equipment = Equipment.Serialise();
            return new Dictionary<String, Variant>()
            {
                { "id", Id },
                { "health", CurrentHealth },
                { "hunger", CurrentHunger },
                { "fatigue", CurrentFatigue },
                { "entertainment", CurrentEntertainment },
                { "stamina", CurrentStamina },
                { "inventory", inventory },
                { "equipment", equipment }
            };
        }


        /// <inheritdoc/>
        public override void Deserialise(Dictionary<String, Variant> data)
        {
            CurrentHealth = (Single)data["health"];
            CurrentHunger = (Single)data["hunger"];
            CurrentFatigue = (Single)data["fatigue"];
            CurrentEntertainment = (Single)data["entertainment"];
            CurrentStamina = (Single)data["stamina"];
            Inventory.Deserialise((Dictionary<Vector2I, Dictionary<String, Variant>>)data["inventory"]);
            Equipment.Deserialise((Dictionary<String, Dictionary<String, Variant>>)data["equipment"]);
        }
    }
}
