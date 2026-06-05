using System;
using System.Collections.Generic;

namespace Khepri.Descriptions
{
    /// <summary> Accumulates spans into a <see cref="Description"/>. Passed to each contributing part (a feature, a component) so it can append its own prose and notes. </summary>
    /// <remarks> The append methods return the builder so contributions can be chained fluently. The builder records spans verbatim and in order; <see cref="Compose"/> handles joining several parts' contributions with a separator. </remarks>
    public sealed class DescriptionBuilder
    {
        /// <summary> The spans accumulated so far, in the order they were appended. </summary>
        private readonly List<DescriptionSpan> _spans = new List<DescriptionSpan>();


        /// <summary> Whether nothing has been appended yet. </summary>
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
            _spans.AddRange(description.Spans);
            return this;
        }


        /// <summary> Produces an immutable <see cref="Description"/> from the spans appended so far. </summary>
        /// <returns> The assembled description. </returns>
        public Description Build() => new Description(_spans.ToArray());


        /// <summary> Runs each contribution into its own builder and joins the non-empty results with a separator. </summary>
        /// <remarks> The shared assembly step behind a room composing its features and entities, and an entity composing its components: a contribution that adds nothing is skipped, so no stray separators appear around it. </remarks>
        /// <param name="contributions"> The contributing callbacks, in the order they should appear. </param>
        /// <param name="separator"> The text inserted between consecutive non-empty contributions. </param>
        /// <returns> The joined description. </returns>
        public static Description Compose(IEnumerable<Action<DescriptionBuilder>> contributions, String separator = " ")
        {
            DescriptionBuilder result = new DescriptionBuilder();

            foreach (Action<DescriptionBuilder> contribute in contributions)
            {
                DescriptionBuilder part = new DescriptionBuilder();
                contribute(part);

                if (!part.IsEmpty)
                {
                    if (!result.IsEmpty) { result.Text(separator); }
                    result.Append(part.Build());
                }
            }

            return result.Build();
        }
    }
}
