using System;
using System.Linq;

namespace Khepri.Descriptions
{
    /// <summary> Assembles an entity's display name from a noun and adjectives. </summary>
    public sealed class NameBuilder
    {
        /// <summary> The first non-empty noun claimed, or <c>null</c> until one is accepted. </summary>
        private String _noun;

        /// <summary> The adjectives decorating the noun. Their order is determined by the royal order of their kinds. </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>1. Determiner (a, an, the, this, your, two)</description>
        ///         </item>
        ///         <item>
        ///             <description>2. Opinion (beautiful, delicious, strange, lovely)</description>
        ///         </item>
        ///         <item>
        ///             <description>3. Size (big, small, tiny)</description>
        ///         </item>
        ///         <item>
        ///             <description>4. Physical Quality (rough, thin)</description>
        ///         </item>
        ///         <item>
        ///             <description>5. Shape (round, square, triangular)</description>
        ///         </item>
        ///         <item>
        ///             <description>6. Age (old, new, ancient)</description>
        ///         </item>
        ///         <item>
        ///             <description>7. Color (red, blue, green)</description>
        ///         </item>
        ///         <item>
        ///             <description>8. Origin (French, Australian, lunar)</description>
        ///         </item>
        ///         <item>
        ///             <description>9. Material (wooden, metal, cotton)</description>
        ///         </item>
        ///         <item>
        ///             <description>10. Purpose/Qualifier  (wedding [dress], sleeping [bag])</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        private readonly String?[] _adjectives = new String?[10];

        internal NameBuilder(String noun)
        {
            _noun = noun;
        }


        /// <summary> Claims the entity's noun; the first non-empty claim wins and later claims are silently ignored. Arbitrating between differing nouns when multiple parts compete is the caller's responsibility, not this assembler's. </summary>
        /// <param name="noun"> The noun to claim; blank claims are ignored. </param>
        /// <returns> This builder, for chaining. </returns>
        public static NameBuilder Create(String noun)
        {
            return new NameBuilder(noun);
        }


        /// <summary> Adds an adjective that decorates the noun. Adjectives appear in the order added, so the caller is responsible for any English ordering. </summary>
        /// <param name="royalOrder"> The royal order of the adjective. </param>
        /// <param name="adjective"> The adjective to add; blank adjectives are ignored. </param>
        /// <returns> This builder, for chaining. </returns>
        public NameBuilder WithAdjective(Int32 royalOrder, String adjective)
        {
            // Ignore blank claims and claims with an invalid royal order; the caller is responsible for any English ordering.
            if (!String.IsNullOrEmpty(adjective) && royalOrder >= 0 && royalOrder < _adjectives.Length)
            {
                _adjectives[royalOrder] = adjective;
            }
            return this;
        }


        /// <summary> Composes the accumulated claims into a display name. </summary>
        /// <returns> The ordered adjectives followed by the noun, or the fallback label when no noun was claimed. </returns>
        public String Build() => String.Join(" ", _adjectives.Where(a => a != null).Append(_noun));
    }
}
