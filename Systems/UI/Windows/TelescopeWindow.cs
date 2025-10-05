using Godot;
using Khepri.Controllers;
using Khepri.Entities.Devices;
using Khepri.Models;
using Khepri.Types.Extensions;
using System;

namespace Khepri.UI.Windows
{
    /// <summary> A window to represent the view through a telescope. </summary>
    public partial class TelescopeWindow : Control
    {
        /// <summary> The telescope's local azimuth. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _lookAzimuth;
        [ExportSubgroup("Labels")]

        /// <summary> The telescope's local altitude. </summary>
        [Export] private RichTextLabel _lookAltitude;

        /// <summary> The telescope's X position in celestial coordinates. </summary>
        [Export] private RichTextLabel _lookRightAscension;

        /// <summary> The telescope's Y position in celestial coordinates. </summary>
        [Export] private RichTextLabel _lookDeclination;

        /// <summary> The unique identifier of the currently targeted star. </summary>
        [Export] private RichTextLabel _starId;

        /// <summary> The proper / common name of the currently targeted star. </summary>
        [Export] private RichTextLabel _starProperName;

        /// <summary> The right ascension of the currently targeted star. </summary>
        [Export] private RichTextLabel _starRightAscension;

        /// <summary> The declination of the currently targeted star. </summary>
        [Export] private RichTextLabel _starDeclination;


        /// <summary> The format to use for azimuth and right ascension labels. </summary>
        private const String HORIZONTAL_FORMAT = "[color=red]{0}: {1:000.000}°";

        /// <summary> The format to use for altitude and declination labels. </summary>
        private const String VERTICAL_FORMAT = "[color=red]{0}: {1:+00.000;-00.000}°";


        /// <summary> A reference to the telescope the player is currently using. </summary>
        private Telescope? _currentTelescope = null;

        /// <summary> A reference to the world location controller. </summary>
        private WorldController _worldController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;
        }


        /// <summary> Initialise the window. </summary>
        /// <param name="telescope"> The telescope the window is attached to. </param>
        public void Initialise(Telescope telescope)
        {
            _currentTelescope = telescope;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (Visible && _currentTelescope != null)
            {
                Vector2 equatorial = MathExtensions.ConvertToEquatorial(_currentTelescope.Azimuth, _currentTelescope.Altitude, _worldController.Latitude, _worldController.LocalSiderealTime);
                _lookAzimuth.Text = String.Format(HORIZONTAL_FORMAT, "AZI", _currentTelescope.Azimuth);
                _lookAltitude.Text = String.Format(VERTICAL_FORMAT, "ALT", _currentTelescope.Altitude);
                _lookRightAscension.Text = String.Format(HORIZONTAL_FORMAT, "RGA", equatorial.X);
                _lookDeclination.Text = String.Format(VERTICAL_FORMAT, "DEC", equatorial.Y);

                StarData closestStar = _currentTelescope.ClosestStar;
                _starId.Text = closestStar.GetId();
                _starProperName.Text = closestStar.Proper;
                _starRightAscension.Text = String.Format(HORIZONTAL_FORMAT, "RGA", Mathf.RadToDeg(closestStar.RightAscension));
                _starDeclination.Text = String.Format(VERTICAL_FORMAT, "DEC", Mathf.RadToDeg(closestStar.Declination));
            }
        }
    }
}
