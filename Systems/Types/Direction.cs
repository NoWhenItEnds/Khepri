using Godot;
using System;

namespace Khepri.Types
{
    /// <summary> Represents a six-point direction. </summary>
    public enum Direction
    {
        NONE,
        UP,
        RIGHT,
        DOWN,
        LEFT,
        TOP,
        BOTTOM
    }


    /// <summary> Helper methods for working with the direction type. </summary>
    public static class DirectionExtensions
    {
        /// <summary> Converts a vector direction to the enum type. </summary>
        /// <param name="vector"> The direction as a vector. </param>
        /// <returns> The closest six-point direction. </returns>
        public static Direction ToDirection(this Vector3 vector)
        {
            // First check if there is an up or down direction.
            if (vector.Y != 0f)
            {
                return vector.Y > 0f ? Direction.TOP : Direction.BOTTOM;
            }

            // Convert angle to something usable.
            Vector2 direction2D = new Vector2(vector.X, vector.Z);
            Single angle = direction2D.Angle();
            if (angle < 0f)
            {
                angle += Mathf.Tau;
            }
            angle = Mathf.RadToDeg(angle);

            Direction result = Direction.NONE;
            if (angle > 45f && angle <= 135f)
            {
                result = Direction.DOWN;
            }
            else if (angle > 135f && angle <= 225f)
            {
                result = Direction.LEFT;
            }
            else if (angle > 225f && angle <= 315f)
            {
                result = Direction.UP;
            }
            else
            {
                result = Direction.RIGHT;
            }
            return result;
        }
    }
}
