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
        /// <param name="text"> The text to capitalise. </param>
        /// <returns> The capitalised text, or the original when it is empty. </returns>
        public static String ToCapitalised(this String text) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLowerInvariant());


        /// <summary> Converts <paramref name="input"/> to a canonical lowercase snake_case string according to the following rules (applied in order). </summary>
        /// <remarks>
        /// Normalisation rules applied in order:
        /// <list type="number">
        ///   <item><description>Spaces and hyphens are treated as word separators and replaced with <c>_</c>.</description></item>
        ///   <item><description>A transition from a lowercase letter (or digit) to an uppercase letter inserts <c>_</c> before the uppercase letter: <c>maxHealth</c> → <c>max_health</c>.</description></item>
        ///   <item><description>An acronym run (consecutive uppercase letters) followed by a lowercase letter splits before the last capital, so the final capital begins the next word: <c>HTTPServer</c> → <c>http_server</c>, <c>MaxHP</c> → <c>max_hp</c>.</description></item>
        ///   <item><description>Digits do not split from an adjacent lowercase letter or digit: <c>vector2</c> → <c>vector2</c>, <c>health2</c> → <c>health2</c>. However, a digit immediately followed by an uppercase letter is treated as a camelCase word boundary and does split: <c>slot1Item</c> → <c>slot1_item</c>, <c>Vector2D</c> → <c>vector2_d</c>.</description></item>
        ///   <item><description>All characters are lowercased.</description></item>
        ///   <item><description>Repeated underscores are collapsed to a single <c>_</c>.</description></item>
        ///   <item><description>Leading and trailing underscores are trimmed.</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <c>"HealthComponent".ToSnakeCase()</c> → <c>"health_component"</c><br/>
        /// <c>"maxHealth".ToSnakeCase()</c>        → <c>"max_health"</c><br/>
        /// <c>"HTTPServer".ToSnakeCase()</c>       → <c>"http_server"</c><br/>
        /// <c>"max-health".ToSnakeCase()</c>       → <c>"max_health"</c><br/>
        /// <c>"max_health".ToSnakeCase()</c>       → <c>"max_health"</c>
        /// </example>
        /// <param name="input"> The identifier to normalise; must not be null, empty, or whitespace, and must produce a non-empty result after normalisation. </param>
        /// <returns> The canonical snake_case representation of <paramref name="input"/>. </returns>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="input"/> is null, empty, whitespace. </exception>
        public static String ToSnakeCase(this String input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input must not be null, empty, or whitespace.", nameof(input));
            }

            String result = BuildSnakeCase(input);

            if (result.Length == 0)
            {
                throw new ArgumentException($"Input '{input}' normalises to an empty string and is therefore not meaningful.", nameof(input));
            }

            return result;
        }


        /// <summary> Performs the actual character-by-character snake_case conversion, then collapses duplicate underscores and trims boundary underscores. </summary>
        /// <param name="input"> The validated, non-blank input string. </param>
        /// <returns> The normalised snake_case string, which may be empty if <paramref name="input"/> contained only separators. </returns>
        private static String BuildSnakeCase(String input)
        {
            StringBuilder builder = new StringBuilder(input.Length + 4);

            for (Int32 index = 0; index < input.Length; index++)
            {
                Char current   = input[index];
                Char? next     = index + 1 < input.Length ? input[index + 1] : (Char?)null;
                Char? previous = index > 0                ? input[index - 1] : (Char?)null;

                AppendCharacter(builder, current, previous, next);
            }

            return CollapseAndTrim(builder.ToString());
        }


        /// <summary> Appends one character (or an underscore prefix then the character) to the buffer according to the boundary-detection rules. </summary>
        /// <param name="builder"> The output buffer being assembled. </param>
        /// <param name="current"> The character being processed. </param>
        /// <param name="previous"> The character immediately before <paramref name="current"/>, or <c>null</c> when <paramref name="current"/> is the first character. </param>
        /// <param name="next"> The character immediately after <paramref name="current"/>, or <c>null</c> when <paramref name="current"/> is the last character. </param>
        private static void AppendCharacter(StringBuilder builder, Char current, Char? previous, Char? next)
        {
            Boolean isSeparator = current == ' ' || current == '-';

            if (isSeparator)
            {
                builder.Append('_');
            }
            else if (Char.IsUpper(current))
            {
                Boolean needsSeparatorBefore = NeedsSeparatorBeforeUpper(current, previous, next);

                if (needsSeparatorBefore)
                {
                    builder.Append('_');
                }

                builder.Append(Char.ToLowerInvariant(current));
            }
            else
            {
                builder.Append(current);
            }
        }


        /// <summary> Determines whether an underscore separator must be inserted before an uppercase character, handling both camelCase boundaries and acronym-run boundaries. </summary>
        /// <param name="current"> The uppercase character under consideration. </param>
        /// <param name="previous"> The character before <paramref name="current"/>, or <c>null</c> if at the start. </param>
        /// <param name="next"> The character after <paramref name="current"/>, or <c>null</c> if at the end. </param>
        /// <returns> <c>true</c> when a separator is required; <c>false</c> when <paramref name="current"/> is at the very start of the string or directly follows another separator character. </returns>
        private static Boolean NeedsSeparatorBeforeUpper(Char current, Char? previous, Char? next)
        {
            Boolean isFirstChar         = previous == null;
            Boolean previousIsSeparator = previous == '_' || previous == ' ' || previous == '-';

            Boolean needsSeparator;

            if (isFirstChar || previousIsSeparator)
            {
                needsSeparator = false;
            }
            else
            {
                // camelCase boundary: a lowercase letter (or digit) is immediately before this uppercase.
                Boolean previousIsLowerOrDigit = previous.HasValue && (Char.IsLower(previous.Value) || Char.IsDigit(previous.Value));

                // Acronym-run boundary: this uppercase follows another uppercase but is itself
                // followed by a lowercase — e.g. the 'S' in "HTTPServer" or 'P' in "MaxHP".
                Boolean previousIsUpper = previous.HasValue && Char.IsUpper(previous.Value);
                Boolean nextIsLower     = next.HasValue     && Char.IsLower(next.Value);
                Boolean isEndOfAcronym  = previousIsUpper && nextIsLower;

                needsSeparator = previousIsLowerOrDigit || isEndOfAcronym;
            }

            return needsSeparator;
        }


        /// <summary>  Collapses runs of consecutive underscores to a single <c>_</c> and removes any leading or trailing underscores from the result. </summary>
        /// <param name="raw"> The intermediate string produced by the character loop. </param>
        /// <returns> The cleaned snake_case string. </returns>
        private static String CollapseAndTrim(String raw)
        {
            StringBuilder builder = new StringBuilder(raw.Length);
            Boolean lastWasUnderscore = false;

            foreach (Char ch in raw)
            {
                if (ch == '_')
                {
                    lastWasUnderscore = true;
                }
                else
                {
                    if (lastWasUnderscore && builder.Length > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(ch);
                    lastWasUnderscore = false;
                }
            }

            return builder.ToString();
        }
    }
}
