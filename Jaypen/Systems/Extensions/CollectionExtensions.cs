using System;
using System.Collections.Generic;
using System.Linq;

namespace Jaypen.Extensions
{
    /// <summary> Additional helper methods for working with collections. </summary>
    public static class CollectionExtensions
    {
        /// <summary> Attempts to select a uniformly random element from the collection. </summary>
        /// <typeparam name="T"> The type of the elements in the collection. </typeparam>
        /// <param name="source"> The source collection; must not be null. </param>
        /// <param name="result"> A random element, or the default value of T (probably null) if the collection is empty. </param>
        /// <returns> If an element was selected. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="source"/> is null. </exception>
        public static Boolean TryGetRandomElement<T>(this IEnumerable<T> source, out T? result)
        {
            ArgumentNullException.ThrowIfNull(source);

            IList<T> list = source as IList<T> ?? source.ToList();
            Boolean hasElements = list.Count > 0;
            result = hasElements ? list[Random.Shared.Next(list.Count)] : default(T);
            return hasElements;
        }


        /// <summary> Get the element from an array by requesting a relative index. </summary>
        /// <typeparam name="T"> The kind of element in the array. </typeparam>
        /// <param name="array"> The array to search; must not be null or empty. </param>
        /// <param name="index"> The relative index to get the element of. </param>
        /// <returns> The discovered element. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="array"/> is null. </exception>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="array"/> is empty — wrapping is undefined with no elements. </exception>
        public static T GetWrapped<T>(this T[] array, Int32 index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Length == 0)
            {
                throw new ArgumentException("Cannot get a wrapped element from an empty array.", nameof(array));
            }

            Int32 wrappedIndex = (index % array.Length + array.Length) % array.Length;
            return array[wrappedIndex];
        }


        /// <summary> Attempts to select a random element from the collection, with each element's probability proportional to its weight. </summary>
        /// <typeparam name="T"> The type of the elements in the collection. </typeparam>
        /// <param name="source"> The source collection; must not be null. </param>
        /// <param name="weightSelector"> Returns the non-negative weight of an element; elements with zero weight are never selected. </param>
        /// <param name="result"> The selected element, or the default value of T when no element carries positive weight. </param>
        /// <returns> If an element was selected. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="source"/> or <paramref name="weightSelector"/> is null. </exception>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="weightSelector"/> returns a negative weight. </exception>
        public static Boolean TryGetRandomWeighted<T>(this IEnumerable<T> source, Func<T, Single> weightSelector, out T? result)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(weightSelector);

            IList<T> list = source as IList<T> ?? source.ToList();

            // Weights are cached in one pass so the selector is invoked exactly once per element, keeping expensive or impure selectors safe.
            Single[] weights = new Single[list.Count];
            Single totalWeight = 0f;
            Int32 lastPositiveIndex = -1;

            for (Int32 index = 0; index < list.Count; index++)
            {
                Single weight = weightSelector(list[index]);
                if (weight < 0f)
                {
                    throw new ArgumentException($"Weight selector returned a negative weight ({weight}); weights must be non-negative.", nameof(weightSelector));
                }

                weights[index] = weight;
                totalWeight += weight;

                if (weight > 0f)
                {
                    lastPositiveIndex = index;
                }
            }

            Boolean selected = totalWeight > 0f;
            if (!selected)
            {
                result = default(T);
            }
            else
            {
                Single roll = Random.Shared.NextSingle() * totalWeight;
                Single cumulative = 0f;
                Int32 chosenIndex = -1;

                for (Int32 index = 0; index < list.Count && chosenIndex == -1; index++)
                {
                    cumulative += weights[index];
                    if (roll < cumulative)
                    {
                        chosenIndex = index;
                    }
                }

                // Floating-point rounding can leave the cumulative walk fractionally short of the roll; fall back to the last selectable element.
                result = list[chosenIndex >= 0 ? chosenIndex : lastPositiveIndex];
            }

            return selected;
        }


        /// <summary> Shuffles the list in place so that every permutation is equally likely (Fisher–Yates). </summary>
        /// <typeparam name="T"> The type of the elements in the list. </typeparam>
        /// <param name="list"> The list to shuffle; must not be null. </param>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="list"/> is null. </exception>
        public static void Shuffle<T>(this IList<T> list)
        {
            ArgumentNullException.ThrowIfNull(list);

            for (Int32 index = list.Count - 1; index > 0; index--)
            {
                Int32 swapIndex = Random.Shared.Next(index + 1);
                (list[index], list[swapIndex]) = (list[swapIndex], list[index]);
            }
        }
    }
}
