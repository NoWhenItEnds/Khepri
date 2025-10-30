using System;
using System.Text.Json.Serialization;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;

namespace Khepri.Data.Devices
{
    /// <summary> The data component of a nano forge device. </summary>
    public class NanoForgeData : DeviceData
    {
        /// <summary> The id of the item the nano forge will attempt to spawn. </summary>
        [JsonPropertyName("selected_item")]
        public String SelectedItem { get; private set; } = "item_food_apple";   // TODO - Not this. A method to set.


        /// <inheritdoc/>
        public override void Use(ActorNode activatingBeing)
        {
            ItemController.Instance.CreateItem(SelectedItem, activatingBeing.GlobalPosition);
        }
    }
}
