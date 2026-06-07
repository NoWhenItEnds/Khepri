using System;
using System.Collections.Generic;
using System.Text;
using Khepri.Descriptions;

namespace Khepri.UI.World.Rooms
{
    /// <summary> Translates a <see cref="Description"/> into BBCode for a <c>RichTextLabel</c>. </summary>
    /// <remarks> Interactive renders wrap each <see cref="NoteSpan"/> in a <c>[url]</c> meta and record the id-to-source map, so the panel can resolve a hovered meta back to its <see cref="INoteSource"/>; inert renders (for a tooltip body) style notes but leave them non-interactive. </remarks>
    public static class DescriptionRenderer
    {
        /// <summary> The colour applied to note text, so hoverable items stand out from plain prose. </summary>
        private const String NoteColour = "#d8b46a";


        /// <summary> Renders a description with interactive notes, recording each note's source against the meta key used to identify it on hover. </summary>
        /// <param name="description"> The description to render. </param>
        /// <param name="notes"> Receives a fresh map from meta key to the source backing that note; cleared before use. </param>
        /// <returns> The BBCode string for a <c>RichTextLabel</c>. </returns>
        public static String ToBbcode(Description description, IDictionary<String, INoteSource> notes)
        {
            notes.Clear();
            StringBuilder bbcode = new StringBuilder();
            Int32         index  = 0;

            foreach (DescriptionSpan span in description.Spans)
            {
                if (span is NoteSpan note)
                {
                    String key = index.ToString();
                    notes[key] = note.Source;
                    index++;

                    bbcode.Append("[url=").Append(key).Append("][color=").Append(NoteColour).Append("]")
                          .Append(Escape(note.Text))
                          .Append("[/color][/url]");
                }
                else
                {
                    bbcode.Append(Escape(span.Text));
                }
            }

            return bbcode.ToString();
        }


        /// <summary> Renders a description with notes styled but inert — for a tooltip body, where its notes are not themselves hoverable. </summary>
        /// <param name="description"> The description to render. </param>
        /// <returns> The BBCode string for a <c>RichTextLabel</c>. </returns>
        public static String ToBbcode(Description description)
        {
            StringBuilder bbcode = new StringBuilder();

            foreach (DescriptionSpan span in description.Spans)
            {
                if (span is NoteSpan)
                {
                    bbcode.Append("[color=").Append(NoteColour).Append("]").Append(Escape(span.Text)).Append("[/color]");
                }
                else
                {
                    bbcode.Append(Escape(span.Text));
                }
            }

            return bbcode.ToString();
        }


        /// <summary> Escapes text so a literal opening bracket is not parsed as the start of a BBCode tag. </summary>
        /// <param name="text"> The raw span text. </param>
        /// <returns> The text with opening brackets neutralised. </returns>
        private static String Escape(String text) => text.Replace("[", "[lb]");
    }
}
