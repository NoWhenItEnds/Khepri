using System;
using System.Globalization;

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
    }
}
