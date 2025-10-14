using Godot;
using Khepri.Controllers;
using Khepri.Entities.Devices;
using Khepri.Nodes.Extensions;
using Khepri.Resources.Celestial;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.UI.Windows
{
    /// <summary> A window to represent the view through a telescope. </summary>
    public partial class TelescopeWindow : Control
    {
        /// <summary> A helper camera used for calculating star positions. </summary>
        [ExportGroup("Nodes")]
        [ExportSubgroup("Helpers")]
        [Export] private Camera3D _helperCamera;


        /// <summary> The telescope's local azimuth. </summary>
        [ExportSubgroup("Labels")]
        [Export] private RichTextLabel _lookAzimuth;

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


        /// <summary> How many degrees each pfile of star in the array represents. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _starSegmentSize = 10f;


        /// <summary> A shader to to use for the starfield. </summary>
        [ExportGroup("Resources")]
        [Export] private ShaderMaterial _starfieldShader;


        /// <summary> The world's loaded star data, ordered first by declination, then right ascension. </summary>
        private StarResource[][] _stars = Array.Empty<StarResource[]>();

        /// <summary> A reference to the telescope the player is currently using. </summary>
        private Telescope? _currentTelescope = null;

        /// <summary> A reference to the game's world controller for latitude and longitude. </summary>
        private WorldController _worldController;


        /// <summary> The format to use for azimuth and right ascension labels. </summary>
        private const String HORIZONTAL_FORMAT = "[color=#bf0001]{0}: {1:000.000}°";

        /// <summary> The format to use for altitude and declination labels. </summary>
        private const String VERTICAL_FORMAT = "[color=#bf0001]{0}: {1:+00.000;-00.000}°";

        /// <summary> How to format simple text on the label. </summary>
        private const String TEXT_FORMAT = "[color=#bf0001]{0}";


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;
            _stars = StarDataExtensions.Load2DArrayFromFile(_starSegmentSize);
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

                _helperCamera.SetRotationHorizontal(_currentTelescope.Azimuth, _currentTelescope.Altitude);
                (StarResource[] Stars, Vector3[] Positions) visibleStars = GetVisibleStars(equatorial);

                SetStarfieldShader(visibleStars.Positions, visibleStars.Stars.Select(x => x.CalculateColour()).ToArray());
                if (visibleStars.Stars.Length > 0)
                {
                    StarResource closestStar = GetClosestStar(visibleStars.Stars, equatorial);
                    _starId.Text = String.Format(TEXT_FORMAT, closestStar.GetIdentifier());
                    _starProperName.Text = String.Format(TEXT_FORMAT, closestStar.ProperName);
                    _starRightAscension.Text = String.Format(HORIZONTAL_FORMAT, "RGA", Mathf.RadToDeg(closestStar.RightAscension));
                    _starDeclination.Text = String.Format(VERTICAL_FORMAT, "DEC", Mathf.RadToDeg(closestStar.Declination));
                }
            }
        }


        /// <summary> Set the values of the starfield shader. </summary>
        /// <param name="positions"> An array of star positions, and magnitudes. </param>
        /// <param name="colours"> A matching array of star colours. </param>
        private void SetStarfieldShader(Vector3[] positions, Color[] colours)
        {
            _starfieldShader.SetShaderParameter("size", positions.Length);
            _starfieldShader.SetShaderParameter("positions", positions);
            _starfieldShader.SetShaderParameter("colours", colours);
        }


        /// <summary> Get the star resources visible from a given azimuth and altitude. </summary>
        /// <param name="equatorial"> The observing telescope's rotation in relation to the celestial sphere. </param>
        /// <returns> All the stars that are currently on the screen, and their positions within screen space. </returns>
        private (StarResource[] Stars, Vector3[] Positions) GetVisibleStars(Vector2 equatorial)
        {
            StarResource[] nearbyStars = _stars.FilterData(equatorial.Y);

            List<StarResource> visibleStars = new List<StarResource>();
            List<Vector3> positions = new List<Vector3>();
            Rect2 viewportRect = GetViewportRect();
            foreach (StarResource star in nearbyStars)
            {
                Vector2 horizontal = MathExtensions.ConvertToHorizontal(Mathf.RadToDeg(star.RightAscension), Mathf.RadToDeg(star.Declination), _worldController.Latitude, _worldController.LocalSiderealTime);
                Vector2 screenPosition = _helperCamera.GetScreenPosition(horizontal.X, horizontal.Y);

                if (viewportRect.HasPoint(screenPosition))
                {
                    visibleStars.Add(star);
                    positions.Add(new Vector3(screenPosition.X, screenPosition.Y, star.Magnitude));
                }
            }

            return (visibleStars.ToArray(), positions.ToArray());
        }


        /// <summary> Gets the star closest to the given coordinates. </summary>
        /// <param name="stars"> The array of stars to check. </param>
        /// <param name="equatorial"> The observing telescope's rotation in relation to the celestial sphere. </param>
        /// <returns> The star closest to the given coordinate. </returns>
        private StarResource GetClosestStar(StarResource[] stars, Vector2 equatorial)
        {
            // Ensure that there are stars in the array.
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stars.Length);

            StarResource closestStar = stars[0];
            Single previousDistance = equatorial.DistanceTo(new Vector2((Single)stars[0].RightAscension, (Single)stars[0].Declination));
            foreach (StarResource star in stars)
            {
                Single currentDistance = equatorial.DistanceTo(new Vector2((Single)Mathf.RadToDeg(star.RightAscension), (Single)Mathf.RadToDeg(star.Declination)));
                if (currentDistance < previousDistance)
                {
                    previousDistance = currentDistance;
                    closestStar = star;
                }
            }
            return closestStar;
        }
    }
}
