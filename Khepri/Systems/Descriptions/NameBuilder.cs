using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Descriptions
{
    /// <summary> Assembles an entity's display name from the noun a contributor claims, preceded by adjectives in the order they were added. </summary>
    public sealed class NameBuilder
    {
        /// <summary> The label used when no component claimed a noun. </summary>
        private const String Fallback = "something";


        /// <summary> The first non-empty noun claimed, or <c>null</c> until one is accepted. </summary>
        private String? _noun;

        /// <summary> The adjectives decorating the noun, in the order they were added. </summary>
        private readonly List<String> _adjectives = new List<String>();


        /// <summary> Claims the entity's noun; the first non-empty claim wins and later claims are silently ignored. Arbitrating between differing nouns when multiple parts compete is the caller's responsibility, not this assembler's. </summary>
        /// <param name="noun"> The noun to claim; blank claims are ignored. </param>
        /// <returns> This builder, for chaining. </returns>
        public NameBuilder Noun(String noun)
        {
            if (!String.IsNullOrEmpty(noun) && _noun is null)
            {
                _noun = noun;
            }
            return this;
        }


        /// <summary> Adds an adjective that decorates the noun. Adjectives appear in the order added, so the caller is responsible for any English ordering. </summary>
        /// <param name="adjective"> The adjective to add; blank adjectives are ignored. </param>
        /// <returns> This builder, for chaining. </returns>
        public NameBuilder Adjective(String adjective)
        {
            if (!String.IsNullOrEmpty(adjective))
            {
                _adjectives.Add(adjective);
            }
            return this;
        }


        /// <summary> Composes the accumulated claims into a display name. </summary>
        /// <returns> The ordered adjectives followed by the noun, or the fallback label when no noun was claimed. </returns>
        public String Build()
        {
            String result = _noun is null
                ? Fallback
                : String.Join(" ", _adjectives.Append(_noun));
            return result;
        }
    }
}
