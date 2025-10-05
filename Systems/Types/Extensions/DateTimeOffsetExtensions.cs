using System;
using Khepri.Types.Extensions;

namespace Aphelion.Types.Extensions
{
    /// <summary> Useful extensions to the DateTimeOffset class. </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <summary> Converts a given date to the number of days since Midday on November 24, 4714 B.C. </summary>
        /// <param name="value"> The date time object to convert. </param>
        /// <returns> The partial days since Midday on November 24, 4714 B.C. </returns>
        public static Double ToJulianDay(this DateTimeOffset value)
        {
            return value.ToUniversalTime().DateTime.ToOADate() + 2415018.5;
        }


        /// <summary> Convert a time to the sidereal time at Greenwich. </summary>
        /// <param name="value"> The time to convert. </param>
        /// <returns> The locational time at GMT. </returns>
        public static Double ToGMSiderealTime(this DateTimeOffset value)
        {
            Double julianDay = value.ToJulianDay();
            Double julianCenturies = (julianDay - 2451545.0) / 36525.0; // (JD - days since J2000) / days in a year.
            Double result = 280.46061837 + 360.98564736629 * (julianDay - 2451545.0) + (0.000387933 * julianCenturies * julianCenturies) - (julianCenturies * julianCenturies * julianCenturies / 38710000.0);
            return MathExtensions.WrapValue(result, 360);
        }


        /// <summary> Convert a time to the local sidereal time at a given location. </summary>
        /// <param name="value"> The time to convert. </param>
        /// <param name="longitude"> The location's longitude. </param>
        /// <returns> The local locational time. </returns>
        public static Double ToLocalSiderealTime(this DateTimeOffset value, Double longitude)
        {
            Double result = value.ToGMSiderealTime() + longitude;
            return MathExtensions.WrapValue(result, 360);
        }
    }
}
