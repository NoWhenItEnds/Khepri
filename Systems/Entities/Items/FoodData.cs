using Godot;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A piece of data representing a food item. </summary>
    public class FoodData : ItemData
    {
        /// <inheritdoc/>
        public override ItemType ItemType => ItemType.FOOD;

        /// <summary> The amount of health the item recovers. </summary>
        public Single HealthRecovery { get; init; }

        /// <summary> The amount of hunger the item recovers. </summary>
        public Single HungerRecovery { get; init; }

        /// <summary> The amount of fatigue the item recovers. </summary>
        public Single FatigueRecovery { get; init; }

        /// <summary> The amount of entertainment the item recovers. </summary>
        public Single EntertainmentRecovery { get; init; }

        /// <summary> The number of portions the item has remaining. </summary>
        public Int32 Portions { get; private set; }

        /// <inheritdoc/>
        public override Boolean Use(Unit unit)
        {
            Portions -= 1;
            unit.Needs.UpdateHealth(HealthRecovery);
            unit.Needs.UpdateHunger(HungerRecovery);
            unit.Needs.UpdateFatigue(FatigueRecovery);
            unit.Needs.UpdateEntertainment(EntertainmentRecovery);
            return true;
        }
    }
}
