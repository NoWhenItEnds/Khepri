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

            Direction result = Direction.NONE;
            if (angle > 22.5f && angle <= 67.5f)
            {
                result = Direction.SE;
            }
            else if (angle > 67.5f && angle <= 112.5f)
            {
                result = Direction.S;
            }
            else if (angle > 112.5f && angle <= 157.5f)
            {
                result = Direction.SW;
            }
            else if (angle > 157.5f && angle <= 202.5f)
            {
                result = Direction.W;
            }
            else if (angle > 202.5f && angle <= 247.5f)
            {
                result = Direction.NW;
            }
            else if (angle > 247.5f && angle <= 292.5f)
            {
                result = Direction.N;
            }
            else if (angle > 292.5f && angle <= 337.5f)
            {
                result = Direction.NE;
            }
            else
            {
                result = Direction.E;
            }
            return result;
        }


        /// <summary> Converts a vector direction to the enum type. </summary>
        /// <param name="direction"> The direction in degrees. </param>
        /// <returns> The closest four-point direction. </returns>
        public static Direction ToDirection(this Single direction)
        {
            Single angle = Math.Abs(direction);
            Direction result = Direction.NONE;
            if (angle > 45f && angle <= 135f)
            {
                result = Direction.E;
            }
            else if (angle > 135f && angle <= 225f)
            {
                result = Direction.S;
            }
            else if (angle > 225f)
            {
                result = Direction.W;
            }
            else
            {
                result = Direction.N;
            }
            return result;
        }
    }
}
