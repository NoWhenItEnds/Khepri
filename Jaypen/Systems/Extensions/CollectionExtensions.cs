using System;
using System.Collections.Generic;
using System.Linq;

namespace Jaypen.Utilities.Extensions
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
        /// <param name="array"> The array to search. </param>
        /// <param name="index"> The relative index to get the element of. </param>
        /// <returns> The discovered element. </returns>
        public static T GetWrapped<T>(this T[] array, Int32 index)
        {
            Int32 wrappedIndex = (index % array.Length + array.Length) % array.Length;
            return array[wrappedIndex];
        }
    }
}
