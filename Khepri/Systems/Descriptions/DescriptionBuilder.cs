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

        /// <summary> The gap text requested but not yet materialised, or <c>null</c> when none is pending. It is written out only when real content follows it. </summary>
        private String? _pendingGap;


        /// <summary> Whether no real content has been appended yet. A merely pending separator does not count. </summary>
        public Boolean IsEmpty => _spans.Count == 0;


        /// <summary> Requests a word gap before whatever content comes next. The gap materialises only when content follows it, and never opens the description. </summary>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Separator()
        {
            // Never downgrade an already-requested paragraph to a word gap.
            if (_pendingGap is null)
            {
                _pendingGap = WordGap;
            }
            return this;
        }


        /// <summary> Requests a paragraph break before whatever content comes next. The break materialises only when content follows it, and never opens the description. </summary>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Paragraph()
        {
            _pendingGap = ParagraphGap;
            return this;
        }


        /// <summary> Appends a run of plain prose. </summary>
        /// <param name="text"> The text to append. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Text(String text)
        {
            CommitPendingGap();
            _spans.Add(new TextSpan(text));
            return this;
        }


        /// <summary> Appends an item of note: hoverable text backed by a live source for its tooltip. </summary>
        /// <param name="text"> The visible text of the note. </param>
        /// <param name="source"> The live object that backs the note's tooltip. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Note(String text, INoteSource source)
        {
            CommitPendingGap();
            _spans.Add(new NoteSpan(text, source));
            return this;
        }


        /// <summary> Appends authored prose, weaving any brace-marked phrases in as notes. Text wrapped in braces — <c>"A {bronze brazier} smoulders…"</c> — becomes a hoverable <see cref="NoteSpan"/> backed by <paramref name="source"/>; everything else is appended as plain prose. A brace without a closing partner renders literally. </summary>
        /// <param name="text"> The authored prose, with optional <c>{…}</c> note markers. </param>
        /// <param name="source"> The live object that backs every marked note's tooltip. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Prose(String text, INoteSource source)
        {
            Int32 position = 0;
            while (position < text.Length)
            {
                Int32 open  = text.IndexOf('{', position);
                Int32 close = open >= 0 ? text.IndexOf('}', open + 1) : -1;

                if (close < 0)
                {
                    // No further well-formed marker; the remainder (including any stray brace) is plain prose.
                    Text(text.Substring(position));
                    position = text.Length;
                }
                else
                {
                    if (open > position)
                    {
                        Text(text.Substring(position, open - position));
                    }

                    String noteText = text.Substring(open + 1, close - open - 1);
                    if (noteText.Length > 0)
                    {
                        Note(noteText, source);
                    }
                    position = close + 1;
                }
            }
            return this;
        }


        /// <summary> Appends every span of an already-built description. </summary>
        /// <param name="description"> The description whose spans are copied in. </param>
        /// <returns> This builder, for chaining. </returns>
        public DescriptionBuilder Append(Description description)
        {
            // An empty description adds nothing, so it must not consume a pending separator.
            if (description.Spans.Count > 0)
            {
                CommitPendingGap();
                _spans.AddRange(description.Spans);
            }
            return this;
        }


        /// <summary> Writes out any pending gap before content lands. A gap at the very start of the description is discarded rather than written. </summary>
        private void CommitPendingGap()
        {
            if (_pendingGap is not null)
            {
                if (_spans.Count > 0)
                {
                    _spans.Add(new TextSpan(_pendingGap));
                }
                _pendingGap = null;
            }
        }


        /// <summary> Produces an immutable <see cref="Description"/> from the spans appended so far. </summary>
        /// <returns> The assembled description. </returns>
        public Description Build() => new Description(_spans.ToArray());
    }
}
