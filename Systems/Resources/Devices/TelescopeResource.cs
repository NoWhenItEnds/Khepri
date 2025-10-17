using Godot;
using Godot.Collections;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Types.Extensions;
using System;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a telescope device. </summary>
    [GlobalClass]
    public partial class TelescopeResource : DeviceResource, IControllable
    {
        /// <summary> A modifier to increase the sensitivity of the input. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _inputSensitivity = 0.1f;


        /// <summary> The telescope's current altitude. It's up and down value. </summary>
        [ExportGroup("State")]
        [Export] public Single Altitude { get; private set; } = 0f;

        /// <summary> The telescope's current azimuth. It's right to left value. Starts from N and rotates cloak-wise. </summary>
        [Export] public Single Azimuth { get; private set; } = 0f;


        /// <summary> The data component of a telescope device. </summary>
        public TelescopeResource() { }


        /// <inheritdoc/>
        public override void Use(BeingNode activatingBeing)
        {
            if (activatingBeing == PlayerController.Instance.PlayerBeing)
            {
                UIController.Instance.ShowTelescope(this);
                PlayerController.Instance.SetControllable(this);
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


        /// <inheritdoc/>
        public override Dictionary<String, Variant> Serialise()
        {
            return new Dictionary<String, Variant>()
            {
                { "id", Id },
                { "altitude", Altitude },
                { "azimuth", Azimuth }
            };
        }


        /// <inheritdoc/>
        public override void Deserialise(Dictionary<String, Variant> data)
        {
            Altitude = (Single)data["altitude"];
            Azimuth = (Single)data["azimuth"];
        }
    }
}
