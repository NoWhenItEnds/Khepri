using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Descriptions
{
    /// <summary> Assembles an entity's display name from the claims its components contribute: the highest-salience noun, preceded by the adjectives in the order they were added. </summary>
    /// <remarks>
    /// A name is the shortest form of an entity describing itself, and like its description it emerges from whatever components are currently present — so a transformation that swaps components renames the entity for free.
    /// Exactly one noun survives (the most salient claim); adjectives are recorded verbatim in the order given, leaving their English ordering to the caller, just as <see cref="DescriptionBuilder"/> leaves prose order to its contributors.
    /// </remarks>
    public sealed class NameBuilder
    {
        /// <summary> The label used when no component claimed a noun. </summary>
        private const String Fallback = "something";


        /// <summary> The winning noun so far, or <c>null</c> until one is claimed. </summary>
        private String? _noun;

        /// <summary> The salience of the winning noun; a later claim must exceed this to take over. </summary>
        private Int32 _salience = Int32.MinValue;

        /// <summary> The adjectives decorating the noun, in the order they were added. </summary>
        private readonly List<String> _adjectives = new List<String>();


        /// <summary> Claims the entity's noun, taking over only if it is strictly more salient than any noun claimed so far. </summary>
        /// <param name="noun"> The noun to claim; blank claims are ignored. </param>
        /// <param name="salience"> How strongly this noun defines the entity; the highest salience wins. </param>
        /// <returns> This builder, for chaining. </returns>
        public NameBuilder Noun(String noun, Int32 salience = 0)
        {
            if (!String.IsNullOrEmpty(noun) && salience > _salience)
            {
                _noun = noun;
                _salience = salience;
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
        /// <returns> The ordered adjectives followed by the noun, or a fallback label when no noun was claimed. </returns>
        public String Build()
        {
            if (_noun is null)
            {
                return Fallback;
            }

            return String.Join(" ", _adjectives.Append(_noun));
        }
    }
}
