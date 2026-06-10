using System;
using System.Collections.Generic;

namespace Khepri.Descriptions
{
    /// <summary> Accumulates spans into a <see cref="Description"/>. Passed to each contributing part (a feature, a component) so it can append its own prose and notes. </summary>
    /// <remarks> The append methods return the builder so contributions can be chained fluently. The builder records spans verbatim and in order. Parts that should read as separate clauses request a <see cref="Separator"/> (or a <see cref="Paragraph"/>) between them: it materialises only when real content follows, so a contribution that adds nothing leaves no stray separator around it. </remarks>
    public sealed class DescriptionBuilder
    {
        /// <summary> The text that joins consecutive contributions in running prose: a single word gap. </summary>
        private const String WordGap = " ";

        /// <summary> The text that sets one contribution apart from the next as its own paragraph: a blank line. </summary>
        private const String ParagraphGap = "\n\n";


        /// <summary> The spans accumulated so far, in the order they were appended. </summary>
        private readonly List<DescriptionSpan> _spans = new List<DescriptionSpan>();


        /// <summary> Whether no real content has been appended yet. A merely pending separator does not count. </summary>
        public Boolean IsEmpty => _spans.Count == 0;


        /// <summary> Appends a run of plain prose. </summary>
        /// <param name="text"> The text to append. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Text(String text)
        {
            _spans.Add(new TextSpan(text));
            return this;
        }


        /// <summary> Appends an item of note: hoverable text backed by a live source for its tooltip. </summary>
        /// <param name="text"> The visible text of the note. </param>
        /// <param name="source"> The live object that backs the note's tooltip. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Note(String text, INoteSource source)
        {
            _spans.Add(new NoteSpan(text, source));
            return this;
        }


        /// <summary> Appends every span of an already-built description. </summary>
        /// <param name="description"> The description whose spans are copied in. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Append(Description description)
        {
            // An empty description adds nothing, so it must not trigger a pending separator.
            if (description.Spans.Count > 0)
            {
                _spans.AddRange(description.Spans);
            }
            return this;
        }


        /// <summary> Produces an immutable <see cref="Description"/> from the spans appended so far. </summary>
        /// <returns> The assembled description. </returns>
        public Description Build() => new Description(_spans.ToArray());
    }
}
