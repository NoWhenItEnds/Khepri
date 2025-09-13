using Godot;
using System;

namespace Khepri.Types.Extensions
{
    /// <summary> Helpful extension methods for working with the Vector3 class. </summary>
    public static class Vector3Extensions
    {
        /// <summary> Whether the position is within range of another. </summary>
        /// <param name="from"> The origin position. </param>
        /// <param name="to"> The target position. </param>
        /// <param name="range"> The acceptable range. </param>
        /// <returns> Whether the target is within range of the origin. </returns>
        public static Boolean InRangeOf(this Vector3 from, Vector3 to, Single range) => from.DistanceTo(to) < range;
    }
}
