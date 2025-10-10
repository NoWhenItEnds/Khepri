using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.Models;
using Khepri.Models.Input;
using Khepri.Types.Extensions;
using System;

namespace Khepri.Entities.Devices
{
    /// <summary> A device a unit can look through to see the night sky. </summary>
    public partial class Telescope : DeviceNode, IControllable
    {
        /// <summary> A modifier to increase the sensitivity of the input. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _inputSensitivity = 0.5f;


        /// <summary> The telescope's current altitude. It's up and down value. </summary>
        public Single Altitude { get; private set; } = 0f;

        /// <summary> The telescope's current azimuth. It's right to left value. Starts from N and rotates cloak-wise. </summary>
        public Single Azimuth { get; private set; } = 0f;

        /// <summary> The star that is currently the closest to the telescope. </summary>
        public StarData ClosestStar => _worldController.GetClosestStar(Azimuth, Altitude);


        /// <summary> A reference to the world location controller. </summary>
        private WorldController _worldController;

        /// <summary> A reference to the player controller. </summary>
        private PlayerController _playerController;

        /// <summary> A reference to the game world's UI controller. </summary>
        private UIController _uiController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;
            _playerController = PlayerController.Instance;
            _uiController = UIController.Instance;

            base._Ready();
        }


        /// <inheritdoc/>
        public void HandleInput(IInput input)
        {
            if (input is MoveInput moveInput)
            {
                Altitude = Math.Clamp(Altitude + (moveInput.Direction.Z * -1f) * _inputSensitivity, 0f, 90f);
                Azimuth += moveInput.Direction.X * _inputSensitivity;
                Azimuth = (Single)MathExtensions.WrapValue(Azimuth, 360);
            }
            else if (input is UseInput useInput)
            {
                throw new NotImplementedException();
            }
        }


        /// <inheritdoc/>
        public override void Examine(Unit activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override void Use(Unit activatingEntity)
        {
            if (activatingEntity == _playerController.PlayerUnit)
            {
                _uiController.ShowTelescope(this);
                _playerController.SetControllable(this);
            }
            else
            {
                // TODO - Let AI fiddle with telescope. Set to homeworld?
            }
        }
    }
}
