using System;
using Godot;

namespace Jaypen.Utilities.Extensions
{
    /// <summary> Helpful methods for working with Vector2. </summary>
    public static class Vector2Extensions
    {
        /// <summary> Returns a random 2-D offset uniformly distributed within a disk. </summary>
        /// <returns> A new random position offset from the original position. </returns>
        /// <remarks> The square-root transform on the radius corrects the centre-bias that arises from sampling radius linearly. Without it positions cluster near the original's position. </remarks>
        public static Vector2 RandomOffset(this Vector2 position, Single spawnRadius)
        {
            Random random = Random.Shared;
            Single angle = (Single)(random.NextSingle() * MathF.PI * 2.0);
            Single radius = (Single)(MathF.Sqrt(random.NextSingle()) * spawnRadius);
            return position + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
        }
    }
}
