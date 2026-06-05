using System;
using System.Collections.Generic;

namespace Khepri.Descriptions
{
    /// <summary> An assembled, ready-to-render description: an ordered sequence of spans built up from a thing's parts. </summary>
    /// <remarks> Returned by anything that describes itself to the player — a room from its features, an entity from its components. The structure (rather than a flat string) is what lets the UI attach tooltips to individual <see cref="NoteSpan"/>s. Immutable: build one with a <see cref="DescriptionBuilder"/>. </remarks>
    public sealed class Description
    {
        /// <summary> A description with no spans, rendered as empty text. </summary>
        public static readonly Description Empty = new Description(Array.Empty<DescriptionSpan>());


        /// <summary> The spans making up this description, in render order. </summary>
        public readonly IReadOnlyList<DescriptionSpan> Spans;


        /// <summary> Initialises a new instance of the <see cref="Description"/> class. </summary>
        /// <param name="spans"> The spans making up the description, in render order. </param>
        public Description(IReadOnlyList<DescriptionSpan> spans)
        {
            Spans = spans;
        }
    }
}
