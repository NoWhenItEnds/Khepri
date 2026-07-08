using System;
using Godot;

namespace Jaypen.Extensions
{
    /// <summary> Helpful methods for working with Vector2. </summary>
    public static class Vector2Extensions
    {
        /// <summary> Returns a copy of the vector with its X component replaced. </summary>
        /// <param name="vector"> The source vector; unchanged, as <see cref="Vector2"/> is immutable. </param>
        /// <param name="x"> The replacement X component. </param>
        /// <returns> A new vector of (<paramref name="x"/>, original Y). </returns>
        public static Vector2 WithX(this Vector2 vector, Single x) => new Vector2(x, vector.Y);


        /// <summary> Returns a copy of the vector with its Y component replaced. </summary>
        /// <param name="vector"> The source vector; unchanged, as <see cref="Vector2"/> is immutable. </param>
        /// <param name="y"> The replacement Y component. </param>
        /// <returns> A new vector of (original X, <paramref name="y"/>). </returns>
        public static Vector2 WithY(this Vector2 vector, Single y) => new Vector2(vector.X, y);



        /// <summary> Returns a random 2-D offset uniformly distributed within a disk. </summary>
        /// <returns> A new random position offset from the original position. </returns>
        /// <remarks> The square-root transform on the radius corrects the centre-bias that arises from sampling radius linearly. Without it positions cluster near the original's position. </remarks>
        public static Vector2 RandomOffset(this Vector2 position, Single spawnRadius)
        {
            Random random = Random.Shared;
            Single angle = random.NextSingle() * MathF.PI * 2.0f;
            Single radius = MathF.Sqrt(random.NextSingle()) * spawnRadius;
            return position + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
        }
    }
}
