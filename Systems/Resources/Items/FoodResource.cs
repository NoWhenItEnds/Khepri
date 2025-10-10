using System;
using Godot;
using Khepri.Entities.Actors;

namespace Khepri.Resources.Items
{
    /// <summary> The data component of a food entity. </summary>
    [GlobalClass]
    public partial class FoodResource : ItemResource
    {
        /// <inheritdoc/>
        public override ItemType ItemType { get; } = ItemType.FOOD;


        /// <summary> The amount of a unit's health the item recovers. </summary>
        [ExportGroup("Statistics")]
        [Export] public Single HealthRecovery { get; private set; }

        /// <summary> The amount of a unit's hunger the item recovers. </summary>
        [Export] public Single HungerRecovery { get; private set; }

        /// <summary> The amount of a unit's fatigue the item recovers. </summary>
        [Export] public Single FatigueRecovery { get; private set; }

        /// <summary> The amount of a unit's entertainment the item recovers. </summary>
        [Export] public Single EntertainmentRecovery { get; private set; }

        /// <summary> The number of portions the item has remaining. </summary>
        [Export] public Int32 Portions { get; private set; }


        /// <summary> The data component of a food entity. </summary>
        public FoodResource() { }


        /// <inheritdoc/>
        public override void Examine(Unit activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override void Use(Unit activatingEntity)
        {
            activatingEntity.Needs.UpdateHealth(HealthRecovery);
            activatingEntity.Needs.UpdateHunger(HungerRecovery);
            activatingEntity.Needs.UpdateFatigue(FatigueRecovery);
            activatingEntity.Needs.UpdateEntertainment(EntertainmentRecovery);

            Portions -= 1;  // TODO - It should queue free.
        }
    }
}
