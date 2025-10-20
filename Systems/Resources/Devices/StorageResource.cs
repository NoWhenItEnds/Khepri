using Godot;
using Godot.Collections;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using System;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a chest or chest-adjacent object. </summary>
    [GlobalClass]
    public partial class StorageResource : DeviceResource
    {
        /// <summary> The grid size of the entity's inventory. </summary>
        [ExportGroup("Settings")]
        [Export] public Vector2I InventorySize { get; private set; } = new Vector2I(10, 10);


        /// <summary> A reference to the entity's inventory component. </summary>
        public EntityInventory Inventory { get; private set; }


        /// <summary> The data component of a chest or chest-adjacent object. </summary>
        public StorageResource()
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


        /// <inheritdoc/>
        public override void Deserialise(Dictionary<String, Variant> data)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override Dictionary<String, Variant> Serialise()
        {
            throw new NotImplementedException();
        }
    }
}
