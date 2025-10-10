using Aphelion.Types.Extensions;
using Godot;
using Khepri.Models;
using Khepri.Nodes.Singletons;
using Khepri.Types.Extensions;
using System;

namespace Khepri.Controllers
{
    /// <summary> Controls the world's state such as its time and seasons. </summary>
    public partial class WorldController : SingletonNode<WorldController>
    {
        /// <summary> The observer's latitude. </summary>
        [ExportGroup("Settings")]
        [Export] public Double Latitude { get; private set; } = -31.80529;

        /// <summary> The observer's longitude. </summary>
        [Export] public Double Longitude { get; private set; } = 115.74419;

        /// <summary> How quickly time is moving. </summary>
        /// <remarks> One in game day takes two hours. </remarks>
        [Export] private Single _timescale = 12f;

        /// <summary> How many degrees each pfile of star in the array represents. </summary>
        [Export] private Single _starSegmentSize = 2f;


        /// <summary> The world's current time. </summary>
        /// <remarks> The game starts on Sunday the 5th of February, 2012. </remarks>
        public DateTimeOffset CurrentTime { get; private set; } = new DateTimeOffset(2012, 2, 5, 0, 0, 0, TimeSpan.FromHours(8));

        /// <summary> The current local sidereal time relative to the observer's longitude. </summary>
        public Double LocalSiderealTime => CurrentTime.ToLocalSiderealTime(Longitude);

        /// <summary> The amount of time (in seconds) that has passed in game time since the previous physics frame. </summary>
        public Double GameTimeDelta { get; private set; }


        /// <summary> The world's loaded star data, ordered first by declination, then right ascension. </summary>
        private StarData[][] _stars = Array.Empty<StarData[]>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            _stars = StarDataExtensions.Load2DArrayFromFile(_starSegmentSize);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            DateTimeOffset previousTime = CurrentTime;
            CurrentTime = CurrentTime.AddSeconds(delta * _timescale);
            GameTimeDelta = (CurrentTime.ToUnixTimeMilliseconds() - previousTime.ToUnixTimeMilliseconds()) * 0.001f;
        }


        /// <summary> Get stars near the given declination. </summary>
        /// <param name="declination"> The declination to search around. </param>
        /// <returns> An array of stars near the given declination. </returns>
        public StarData[] GetStars(Single declination)
        {
            return _stars.FilterData(declination);
        }


        /// <summary> Gets the star closest to the given coordinates. </summary>
        /// <param name="azimuth"> The local azimuth / X position. </param>
        /// <param name="altitude"> The local altitude / Y position. </param>
        /// <returns> The star closest to the given coordinate. </returns>
        public StarData GetClosestStar(Single azimuth, Single altitude)
        {
            Vector2 equatorial = MathExtensions.ConvertToEquatorial(azimuth, altitude, Latitude, LocalSiderealTime);

            StarData[] searchArray = GetStars(equatorial.Y);
            StarData closestStar = searchArray[0];
            Single previousDistance = equatorial.DistanceTo(new Vector2((Single)searchArray[0].RightAscension, (Single)searchArray[0].Declination));
            foreach (StarData star in searchArray)
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
