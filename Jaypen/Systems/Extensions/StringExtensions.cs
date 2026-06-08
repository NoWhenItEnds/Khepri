using System;
using System.Globalization;
using System.Text;

namespace Jaypen.Utilities.Extensions
{
    /// <summary> Extension methods for working with <see cref="String"/>. </summary>
    /// <remarks> All case-folding operations use culture-invariant methods (<c>Char.ToLowerInvariant</c>, <c>Char.IsLower</c>, etc.) to avoid the Turkish-locale case-folding bug. </remarks>
    public static class StringExtensions
    {
        /// <summary> Returns the text with its first character upper-cased, for opening a sentence. </summary>
        /// <param name="input"> The text to capitalise. </param>
        /// <returns> The capitalised text, or the original when it is empty. </returns>
        public static String ToCapitalised(this String input) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());


        /// <summary> Renders a rating as a row of filled and empty dots similar in style to WoD. </summary>
        /// <param name="filled"> The number of earned points to draw filled. </param>
        /// <param name="total"> The total number of points in the rating. </param>
        /// <returns> A string of <paramref name="total"/> dots, the first <paramref name="filled"/> of them filled. </returns>
        public static String BuildDots(Int32 filled, Int32 total)
        {
            StringBuilder builder = new StringBuilder(total);

            for (Int32 point = 0; point < total; point++)
            {
                builder.Append(point < filled ? "●" : "○");
            }

            return builder.ToString();
        }
    }
}
