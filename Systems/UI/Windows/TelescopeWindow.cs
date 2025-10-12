using Godot;
using Khepri.Controllers;
using Khepri.Entities.Devices;
using Khepri.Nodes.Extensions;
using Khepri.Resources.Celestial;
using Khepri.Types;
using Khepri.Types.Extensions;
using Khepri.UI.Windows.Components;
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

        /// <summary> The container to spawn star nodes within. </summary>
        [Export] private Control _starContainer;

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
        [Export] private Single _starSegmentSize = 2f;


        [ExportGroup("Resources")]
        [Export] private ShaderMaterial _starfieldShader;

        [Export] private PackedScene _starPrefab;


        /// <summary> A pool of objects to use for rendering stars. </summary>
        public ObjectPool<TelescopeItem, StarResource> StarPool { get; private set; }


        /// <summary> The world's loaded star data, ordered first by declination, then right ascension. </summary>
        private StarResource[][] _stars = Array.Empty<StarResource[]>();

        /// <summary> A reference to the telescope the player is currently using. </summary>
        private Telescope? _currentTelescope = null;

        /// <summary> A reference to the game's world controller for latitude and longitude. </summary>
        private WorldController _worldController;


        /// <summary> The format to use for azimuth and right ascension labels. </summary>
        private const String HORIZONTAL_FORMAT = "[color=red]{0}: {1:000.000}°";

        /// <summary> The format to use for altitude and declination labels. </summary>
        private const String VERTICAL_FORMAT = "[color=red]{0}: {1:+00.000;-00.000}°";


        /// <inheritdoc/>
        public override void _Ready()
        {
            StarPool = new ObjectPool<TelescopeItem, StarResource>(_starContainer, _starPrefab, 2000);
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

                StarPool.FreeAll();
                _helperCamera.SetRotationHorizontal(_currentTelescope.Azimuth, _currentTelescope.Altitude);
                TelescopeItem[] visibleStars = GetVisibleStars(equatorial);

                if(visibleStars.Length > 0)
                {
                    TelescopeItem closestStar = GetClosestStar(visibleStars, equatorial);
                    _starId.Text = closestStar.Resource.Id;
                    _starProperName.Text = closestStar.Resource.ProperName;
                    _starRightAscension.Text = String.Format(HORIZONTAL_FORMAT, "RGA", Mathf.RadToDeg(closestStar.Resource.RightAscension));
                    _starDeclination.Text = String.Format(VERTICAL_FORMAT, "DEC", Mathf.RadToDeg(closestStar.Resource.Declination));
                }

                QueueRedraw();
            }
        }


        /*
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
        }*/


        /// <summary> Get the star resources visible from a given azimuth and altitude. </summary>
        /// <param name="equatorial"> The observing telescope's rotation in relation to the celestial sphere. </param>
        /// <returns> All the stars that are currently on the screen. </returns>
        private TelescopeItem[] GetVisibleStars(Vector2 equatorial)
        {
            StarResource[] nearbyStars = _stars.FilterData(equatorial.Y);

            List<TelescopeItem> visibleStars = new List<TelescopeItem>();
            Rect2 viewportRect = GetViewportRect();
            foreach (StarResource star in nearbyStars)
            {
                Vector2 horizontal = MathExtensions.ConvertToHorizontal(Mathf.RadToDeg(star.RightAscension), Mathf.RadToDeg(star.Declination), _worldController.Latitude, _worldController.LocalSiderealTime);

                Vector2 screenPosition = _helperCamera.GetScreenPosition(horizontal.X, horizontal.Y);
                if (viewportRect.HasPoint(screenPosition))
                {
                    TelescopeItem item = StarPool.GetAvailableObject();
                    item.Initialise(this, star, screenPosition);
                    visibleStars.Add(item);
                }
            }

            return visibleStars.ToArray();
        }


        public override void _Draw()
        {
            if(_currentTelescope != null)
            {
                Vector2 screenPosition = _helperCamera.GetScreenPosition(_currentTelescope.Azimuth, _currentTelescope.Altitude);
                DrawCircle(screenPosition, 12f, Colors.Red);
            }
        }



        /// <summary> Gets the star closest to the given coordinates. </summary>
        /// <param name="stars"> The array of stars to check. </param>
        /// <param name="equatorial"> The observing telescope's rotation in relation to the celestial sphere. </param>
        /// <returns> The star closest to the given coordinate. </returns>
        private TelescopeItem GetClosestStar(TelescopeItem[] stars, Vector2 equatorial)
        {
            // Ensure that there are stars in the array.
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stars.Length);

            TelescopeItem closestStar = stars[0];
            Single previousDistance = equatorial.DistanceTo(new Vector2((Single)stars[0].Resource.RightAscension, (Single)stars[0].Resource.Declination));
            foreach (TelescopeItem star in stars)
            {
                Single currentDistance = equatorial.DistanceTo(new Vector2((Single)Mathf.RadToDeg(star.Resource.RightAscension), (Single)Mathf.RadToDeg(star.Resource.Declination)));
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
