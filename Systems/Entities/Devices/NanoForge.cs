using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> A device that can spawn objects from nothing but energy. </summary>
    public partial class NanoForge : Device
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
        public override Boolean Examine(Unit activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override Boolean Use(Unit activatingEntity)
        {
            Item newItem = _itemController.CreateItem("apple", ItemType.FOOD, GlobalPosition);
            return true;
        }


        /// <inheritdoc/>
        public override Boolean Grab(Unit activatingEntity)
        {
            throw new NotImplementedException();
        }
    }
}
