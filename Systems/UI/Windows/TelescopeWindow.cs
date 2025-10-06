using Godot;
using Khepri.Controllers;
using Khepri.Entities.Devices;
using Khepri.Models;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [ExportGroup("Resources")]
        [Export] private ShaderMaterial _starfieldShader;


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

                // TODO - Optimise call.
                //StarData closestStar = _currentTelescope.ClosestStar;
                //_starId.Text = closestStar.GetId();
                //_starProperName.Text = closestStar.Proper;
                //_starRightAscension.Text = String.Format(HORIZONTAL_FORMAT, "RGA", Mathf.RadToDeg(closestStar.RightAscension));
                //_starDeclination.Text = String.Format(VERTICAL_FORMAT, "DEC", Mathf.RadToDeg(closestStar.Declination));

                SetStarfieldShader(equatorial.Y);
            }
        }


        private void SetStarfieldShader(Single declination)
        {
            StarData[] stars = _worldController.GetStars(declination);

            Random random = new Random();
            Vector3[] positions = stars
                .Select(s => MathExtensions.ConvertToHorizontal(s.RightAscension, s.Declination, _worldController.Latitude, _worldController.LocalSiderealTime))
                .Select(h => MathExtensions.SphericalToCartesian(h.Y, h.X)).ToArray();

            Color[] colours = new Color[positions.Length];
            for (Int32 i = 0; i < colours.Length; i++)
            {
                colours[i] = new Color(random.NextSingle(), random.NextSingle(), random.NextSingle());
            }

            _starfieldShader.SetShaderParameter("size", positions.Length);
            _starfieldShader.SetShaderParameter("positions", positions);
            _starfieldShader.SetShaderParameter("colours", colours);
        }
    }
}
