using System;
using Godot;
using Khepri.Entities.Actors;
using Khepri.Resources.Actors;

namespace Khepri.Resources.Items
{
    /// <summary> The data component of a food entity. </summary>
    [GlobalClass]
    public partial class FoodResource : ItemResource
    {
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
        public override void Use(Being activatingEntity)
        {
            BeingResource resource = activatingEntity.GetResource<BeingResource>();
            resource.UpdateHealth(HealthRecovery);
            resource.UpdateHunger(HungerRecovery);
            resource.UpdateFatigue(FatigueRecovery);
            resource.UpdateEntertainment(EntertainmentRecovery);

            Portions -= 1;  // TODO - It should queue free.
        }
    }
}
