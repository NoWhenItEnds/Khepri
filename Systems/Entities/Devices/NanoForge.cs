using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> A device that can spawn objects from nothing but energy. </summary>
    public partial class NanoForge : DeviceNode
    {
        /// <summary> A reference to the item controller for spawning items. </summary>
        private ItemController _itemController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _itemController = ItemController.Instance;
            base._Ready();
        }


        /// <inheritdoc/>
        public override void Examine(Unit activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override void Use(Unit activatingEntity)
        {
            _itemController.CreateItem("apple", GlobalPosition);
        }
    }
}
