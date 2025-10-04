using Godot;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> A device a unit can look through to see the night sky. </summary>
    public partial class Telescope : Device
    {
        /// <inheritdoc/>
        public override Boolean Use(IEntity activatingEntity)
        {
            Boolean isSuccessful = false;
            if (activatingEntity is Unit unit)
            {
                throw new NotImplementedException();
            }
            return isSuccessful;
        }
    }
}
