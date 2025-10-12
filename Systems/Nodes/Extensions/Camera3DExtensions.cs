using Godot;
using Khepri.Types.Extensions;
using System;

namespace Khepri.Nodes.Extensions
{
    /// <summary> Extension methods for working with the Camera3D node. </summary>
    public static class Camera3DExtensions
    {
        /// <summary> Set the camera to look at a given horizontal position. </summary>
        /// <param name="camera"> A reference to the camera. </param>
        /// <param name="azimuth"> The position's azimuth. It's value on the X axis. </param>
        /// <param name="altitude"> The position's altitude. It's value on the Y axis. </param>
        public static void SetRotationHorizontal(this Camera3D camera, Double azimuth, Double altitude)
        {
            Single azRad = (Single)Mathf.DegToRad(azimuth);
            Single altRad = (Single)Mathf.DegToRad(altitude);

            Vector3 direction = new Vector3(
                Mathf.Cos(altRad) * Mathf.Sin(azRad),
                Mathf.Sin(altRad),
                Mathf.Cos(altRad) * Mathf.Cos(azRad)
            );

            camera.LookAt(camera.GlobalPosition + direction, Vector3.Up);
        }


        /// <summary> Get the screen position of a horizontal position. </summary>
        /// <param name="camera"> A reference to the camera. </param>
        /// <param name="azimuth"> The position's azimuth. It's value on the X axis. </param>
        /// <param name="altitude"> The position's altitude. It's value on the Y axis. </param>
        /// <returns> The position of the point on the screen. </returns>
        public static Vector2 GetScreenPosition(this Camera3D camera, Double azimuth, Double altitude)
        {
            Single azRad = (Single)Mathf.DegToRad(azimuth);
            Single altRad = (Single)Mathf.DegToRad(altitude);

            Vector3 direction = new Vector3(
                Mathf.Cos(altRad) * Mathf.Sin(azRad),
                Mathf.Sin(altRad),
                Mathf.Cos(altRad) * Mathf.Cos(azRad)
            ).Normalized();

            Vector3 worldPoint = camera.GlobalPosition + direction * 10000f; // Choose a large distance to approximate point at infinity.

            if (camera.IsPositionBehind(worldPoint))
            {
                return Vector2.One * -1f;
            }

            return camera.UnprojectPosition(worldPoint);
        }


        /// <summary> Convert the camera's rotation to the relative altitude and azimuth values. Also known as the horizontal system. </summary>
        /// <param name="camera"> The camera to convert. </param>
        /// <returns> Altitude; Azimuth </returns>
        public static Vector2 ToHorizontal(this Camera3D camera)
        {
            Vector3 direction = -camera.GlobalTransform.Basis.Z;
            Single altitude = (Single)Mathf.RadToDeg(Math.Asin(direction.Y));
            Single azimuth = (Single)Mathf.RadToDeg(Math.Atan2(direction.X, -direction.Z));  // Invert Z for the correct handiness (East should be 90)

            // Normalize azimuth to (0, 360).
            if (azimuth < 0)
                azimuth += 360f;

            return new Vector2(azimuth, altitude);
        }


        /// <summary> Convert the camera's rotation to the equatorial coordinate system. </summary>
        /// <param name="camera"> The camera to convert. </param>
        /// <param name="latitude"> The observer's latitude. </param>
        /// <param name="localSiderealTime"> The observer's local sidereal time, including their longitude. </param>
        /// <returns> The coordinates of a celestial object in degrees. </returns>
        /// <remarks> https://stackoverflow.com/questions/70977467/wrong-result-calculation-hour-angle-from-altitude-azimuth </remarks>
        public static Vector2 ToEquatorial(this Camera3D camera, Double latitude, Double localSiderealTime)
        {
            Vector2 local = camera.ToHorizontal();
            Double az = Mathf.DegToRad(local.X);
            Double alt = Mathf.DegToRad(local.Y);
            Double lat = Mathf.DegToRad(latitude);
            Double lst = Mathf.DegToRad(localSiderealTime);

            Double dec = Math.Asin(Math.Sin(alt) * Math.Sin(lat) + Math.Cos(alt) * Math.Cos(lat) * Math.Cos(az));
            Double ha = Math.Atan2(-Math.Sin(az) * Math.Cos(alt) / Math.Cos(dec), (Math.Sin(alt) - Math.Sin(dec) * Math.Sin(lat)) / (Math.Cos(dec) * Math.Cos(lat)));
            Double ra = lst - ha;

            return new Vector2((Single)MathExtensions.WrapValue(Mathf.RadToDeg(ra), 360f), (Single)Mathf.RadToDeg(dec));
        }
    }
}
