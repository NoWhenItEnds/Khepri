using System;

namespace Jaypen.Extensions
{
    /// <summary> Helpful methods for working with random number generators. </summary>
    public static class RandomExtensions
    {
        /// <summary> Rolls against a probability: returns <see langword="true"/> with the given likelihood. </summary>
        /// <param name="random"> The random number generator to roll with; must not be null. </param>
        /// <param name="probability"> The likelihood of success in the range [0, 1]. Values at or below 0 never succeed; values at or above 1 always succeed. </param>
        /// <returns> If the roll succeeded. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="random"/> is null. </exception>
        public static Boolean Chance(this Random random, Single probability)
        {
            ArgumentNullException.ThrowIfNull(random);

            return random.NextSingle() < probability;
        }


        /// <summary> Returns a random value uniformly distributed in the given range. </summary>
        /// <param name="random"> The random number generator to sample from; must not be null. </param>
        /// <param name="minInclusive"> The inclusive lower bound of the range. </param>
        /// <param name="maxExclusive"> The exclusive upper bound of the range. </param>
        /// <returns> A value in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>). </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="random"/> is null. </exception>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="maxExclusive"/> is less than <paramref name="minInclusive"/>. </exception>
        public static Single NextSingle(this Random random, Single minInclusive, Single maxExclusive)
        {
            ArgumentNullException.ThrowIfNull(random);

            if (maxExclusive < minInclusive)
            {
                throw new ArgumentException($"The upper bound ({maxExclusive}) must not be less than the lower bound ({minInclusive}).", nameof(maxExclusive));
            }

            return minInclusive + random.NextSingle() * (maxExclusive - minInclusive);
        }
    }
}
