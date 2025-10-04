using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> A device a unit can look through to see the night sky. </summary>
    public partial class Telescope : Device
    {
        /// <summary> A reference to the player controller. </summary>
        private PlayerController _playerController;

        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
        }


        /// <inheritdoc/>
        public override Boolean Use(IEntity activatingEntity)
        {
            Boolean isSuccessful = false;
            if (activatingEntity is Unit unit)
            {
                if (unit == _playerController.PlayerUnit)
                {
                    // TODO - Activate telescope window.
                }
                else
                {
                    // TODO - Let AI fiddle with telescope. Set to homeworld?
                }
            }
            return isSuccessful;
        }
    }
}
