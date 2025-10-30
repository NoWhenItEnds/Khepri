using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Khepri.Data.Devices
{
    /// <summary> The data component of a chest or chest-adjacent object. </summary>
    public class StorageData : DeviceData
    {
        /// <summary> The grid size of the entity's inventory. </summary>
        [JsonPropertyName("inventory_size"), Required]
        public required Vector2I InventorySize { get; init; } = new Vector2I(10, 10);


        /// <summary> A reference to the entity's inventory component. </summary>
        public EntityInventory Inventory { get; private set; }


        /// <summary> The data component of a chest or chest-adjacent object. </summary>
        public StorageData() : base()
        {
            Inventory = new EntityInventory(InventorySize);
        }


        /// <inheritdoc/>
        public override void Use(ActorNode activatingBeing)
        {
            if (activatingBeing == ActorController.Instance.GetPlayer())
            {
                UIController.Instance.ShowInventoryTransfer(Inventory);
            }
            else
            {
                // TODO - Let AI get items out.
            }
        }
    }
}
