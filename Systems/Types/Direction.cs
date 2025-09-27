using Godot;
using System;

namespace Khepri.Types
{
    /// <summary> Represents a six-point direction. </summary>
    public enum Direction
    {
        NONE = -1,
        N = 0,
        NE = 1,
        E = 2,
        SE = 3,
        S = 4,
        SW = 5,
        W = 6,
        NW = 7
    }


    /// <summary> Helper methods for working with the direction type. </summary>
    public static class DirectionExtensions
    {
        /// <summary> Converts a vector direction to the enum type. </summary>
        /// <param name="vector"> The direction as a vector. </param>
        /// <returns> The closest four-point direction. </returns>
        public static Direction ToDirection(this Vector3 vector)
        {
            // Convert angle to something usable.
            Vector2 direction2D = new Vector2(vector.X, vector.Z);
            Single angle = direction2D.Angle();
            if (angle < 0f)
            {
                angle += Mathf.Tau;
            }
            angle = Mathf.RadToDeg(angle);

            return angle.ToDirection();
        }


        /// <summary> Converts a vector direction to the enum type. </summary>
        /// <param name="direction"> The direction in degrees. </param>
        /// <returns> The closest four-point direction. </returns>
        public static Direction ToDirection(this Single direction)
        {
            Direction result = Direction.NONE;
            if (direction > 22.5f && direction <= 67.5f)
            {
                result = Direction.SE;
            }
            else if (direction > 67.5f && direction <= 112.5f)
            {
                result = Direction.S;
            }
            else if (direction > 112.5f && direction <= 157.5f)
            {
                result = Direction.SW;
            }
            else if (direction > 157.5f && direction <= 202.5f)
            {
                result = Direction.W;
            }
            else if (direction > 202.5f && direction <= 247.5f)
            {
                result = Direction.NW;
            }
            else if (direction > 247.5f && direction <= 292.5f)
            {
                result = Direction.N;
            }
            else if (direction > 292.5f && direction <= 337.5f)
            {
                result = Direction.NE;
            }
            else
            {
                result = Direction.E;
            }
            return result;
        }
    }
}
