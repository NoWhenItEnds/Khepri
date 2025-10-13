using System;
using Godot;

namespace Khepri.Types.Extensions
{
    /// <summary> Additional math functions. </summary>
    public static class MathExtensions
    {
        /// <summary> Converts from relative, horizontal coordinates to equatorial ones. </summary>
        /// <param name="azimuth"> The rotation east from north in degrees. </param>
        /// <param name="altitude"> The rotation north from the celestial horizon in degrees. </param>
        /// <param name="latitude"> The observer's latitude. </param>
        /// <param name="localSiderealTime"> The observer's local sidereal time, including their longitude. </param>
        /// <returns> The coordinates of a celestial object in degrees. X = right ascension. Y = declination. </returns>
        /// <remarks> https://stackoverflow.com/questions/70977467/wrong-result-calculation-hour-angle-from-altitude-azimuth </remarks>
        public static Vector2 ConvertToEquatorial(Double azimuth, Double altitude, Double latitude, Double localSiderealTime)
        {
            Double az = Mathf.DegToRad(azimuth);
            Double alt = Mathf.DegToRad(altitude);
            Double lat = Mathf.DegToRad(latitude);
            Double lst = Mathf.DegToRad(localSiderealTime);

            Double dec = Math.Asin(Math.Sin(alt) * Math.Sin(lat) + Math.Cos(alt) * Math.Cos(lat) * Math.Cos(az));
            Double ha = Math.Atan2(-Math.Sin(az) * Math.Cos(alt) / Math.Cos(dec), (Math.Sin(alt) - Math.Sin(dec) * Math.Sin(lat)) / (Math.Cos(dec) * Math.Cos(lat)));
            Double ra = lst - ha;

            return new Vector2((Single)WrapValue(Mathf.RadToDeg(ra), 360), (Single)Mathf.RadToDeg(dec));
        }


        /// <summary> Converts from equatorial coordinates to relative, horizontal ones. </summary>
        /// <param name="rightAscension"> The rotation east from north in degrees. </param>
        /// <param name="declination"> The rotation north from the celestial horizon in degrees. </param>
        /// <param name="latitude"> The observer's latitude. </param>
        /// <param name="localSiderealTime"> The observer's local sidereal time, including their longitude. </param>
        /// <returns> The coordinates of a celestial object relative to the observer in degrees. X = azimuth, Y = altitude. </returns>
        public static Vector2 ConvertToHorizontal(Double rightAscension, Double declination, Double latitude, Double localSiderealTime)
        {
            // Compute hour angle in degrees, normalised to [-180, +180].
            Double haDeg = localSiderealTime - rightAscension;
            haDeg = ((haDeg + 180.0) % 360.0) - 180.0;

            Double haRad = Mathf.DegToRad(haDeg);
            Double decRad = Mathf.DegToRad(declination);
            Double latRad = Mathf.DegToRad(latitude);

            Double sinAlt = Math.Sin(decRad) * Math.Sin(latRad) + Math.Cos(decRad) * Math.Cos(latRad) * Math.Cos(haRad);
            sinAlt = Math.Max(-1.0, Math.Min(1.0, sinAlt)); // Clamp to [-1, 1] to handle floating-point precision issues.

            Double altRad = Math.Asin(sinAlt);
            Double altDeg = Mathf.RadToDeg(altRad);

            // Use sqrt for cosAlt to ensure it's non-negative.
            Double cosAlt = Math.Sqrt(1.0 - sinAlt * sinAlt);

            // At zenith or nadir, azimuth is undefined; set to 0 by convention.
            Double azDeg = 0.0;

            if (cosAlt >= 1e-6) // Greater than epsilon.
            {
                Double sinAz = -Math.Cos(decRad) * Math.Sin(haRad) / cosAlt;
                Double cosAz = (Math.Sin(decRad) * Math.Cos(latRad) - Math.Cos(decRad) * Math.Cos(haRad) * Math.Sin(latRad)) / cosAlt;
                azDeg = Mathf.RadToDeg(Math.Atan2(sinAz, cosAz));
                if (azDeg < 0.0) azDeg += 360.0;    // Normalise azimuth to 0-360.
            }

            return new Vector2((Single)azDeg, (Single)altDeg);
        }


        /// <summary> Clamps an value and ensures it wraps. </summary>
        /// <param name="value"> The value to wrap. </param>
        /// <param name="max"> The maximum value before it wraps around from zero. </param>
        /// <returns> The clamped value. </returns>
        public static Double WrapValue(Double value, Double max)
        {
            Double remainder = value % max;
            if (remainder < 0)
            {
                remainder += max;
            }
            return remainder;
        }


        /// <summary> Convert spherical, horizontal coordinates to a position in world space. </summary>
        /// <param name="azimuth"> The object's azimuth. Its rotation on the X axis. </param>
        /// <param name="altitude"> The object's altitude. Its rotation on the Y axis. Also known as theta. </param>
        /// <param name="radius"> The radius of the sphere to map the coordinates to. By default a unit sphere. </param>
        /// <returns> The location of the object in Godot-space. </returns>
        public static Vector3 SphericalToCartesian(Single azimuth, Single altitude, Single radius = 1f)
        {
            Single sinTheta = Mathf.Sin(altitude);
            return new Vector3(
                sinTheta * Mathf.Sin(azimuth),
                Mathf.Cos(altitude),
                sinTheta * Mathf.Cos(azimuth)
            ) * radius;
        }
    }
}
