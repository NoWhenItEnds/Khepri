using Godot;

namespace Khepri.Descriptions
{
    /// <summary> Something a <see cref="NoteSpan"/> can point at — the live object that supplies a hovered note's tooltip. </summary>
    public interface INoteSource
    {
        /// <summary> Builds the description shown in this source's tooltip when one of its notes is hovered. </summary>
        /// <returns> The tooltip body, composed the same way as any other description. </returns>
        Description BuildDescription();


        /// <summary> An optional image representing this source, for the tooltip's display panel. </summary>
        /// <returns> A texture that represents the source, or <c>null</c> when it has no image. </returns>
        Texture2D? GetTexture() => null;
    }
}
