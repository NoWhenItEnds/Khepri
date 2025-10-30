using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Types.Extensions;
using System;
using System.Text.Json.Serialization;

namespace Khepri.Data.Devices
{
    /// <summary> The data component of a telescope device. </summary>
    public class TelescopeData : DeviceData, IControllable
    {
        /// <summary> A modifier to increase the sensitivity of the input. </summary>
        private Single _inputSensitivity = 0.1f;   // TODO - Pull from config controller or something.


        /// <summary> The telescope's current altitude. It's up and down value. </summary>
        [JsonPropertyName("altitude")]
        public Single Altitude { get; private set; } = 0f;

        /// <summary> The telescope's current azimuth. It's right to left value. Starts from N and rotates cloak-wise. </summary>
        [JsonPropertyName("azimuth")]
        public Single Azimuth { get; private set; } = 0f;


        /// <inheritdoc/>
        public override void Use(ActorNode activatingBeing)
        {
            if (activatingBeing == ActorController.Instance.GetPlayer())
            {
                UIController.Instance.ShowTelescope(this);
                ActorController.Instance.SetPlayerControllable(this);
            }
            else
            {
                // TODO - Let AI fiddle with telescope. Set to homeworld?
            }
        }


        /// <inheritdoc/>
        public void HandleInput(IInput input)
        {
            if (input is MoveInput moveInput)
            {
                Altitude = Math.Clamp(Altitude + (moveInput.Direction.Z * -1f) * _inputSensitivity, 0f, 90f);
                Azimuth += (moveInput.Direction.X * -1f) * _inputSensitivity;
                Azimuth = (Single)MathExtensions.WrapValue(Azimuth, 360);
            }
            else if (input is UseInput useInput)
            {
                throw new NotImplementedException();
            }
        }
    }
}
