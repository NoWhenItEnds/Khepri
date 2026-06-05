using System;

namespace Khepri.Descriptions
{
    /// <summary> A single contiguous run of a <see cref="Description"/>. Either plain prose (<see cref="TextSpan"/>) or a hoverable item of note (<see cref="NoteSpan"/>). </summary>
    public abstract class DescriptionSpan
    {
        /// <summary> The literal text this span contributes to the rendered description. </summary>
        public readonly String Text;


        /// <summary> Initialises a new instance of the <see cref="DescriptionSpan"/> class. </summary>
        /// <param name="text"> The literal text this span contributes. </param>
        protected DescriptionSpan(String text)
        {
            Text = text;
        }
    }


    /// <summary> A run of plain prose, rendered as-is with no interactivity. </summary>
    public sealed class TextSpan : DescriptionSpan
    {
        /// <summary> Initialises a new instance of the <see cref="TextSpan"/> class. </summary>
        /// <param name="text"> The prose to render. </param>
        public TextSpan(String text) : base(text) { }
    }


    /// <summary> An item of note: text the player can hover to reveal a tooltip sourced from <see cref="Source"/>. </summary>
    public sealed class NoteSpan : DescriptionSpan
    {
        /// <summary> The live object that supplies this note's tooltip when hovered. </summary>
        public readonly INoteSource Source;


        /// <summary> Initialises a new instance of the <see cref="NoteSpan"/> class. </summary>
        /// <param name="text"> The visible text of the note. </param>
        /// <param name="source"> The live object that backs the note's tooltip. </param>
        public NoteSpan(String text, INoteSource source) : base(text)
        {
            Source = source;
        }
    }
}
