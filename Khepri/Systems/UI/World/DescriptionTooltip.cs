using System;
using Godot;
using Khepri.Descriptions;

namespace Khepri.UI.World
{
    /// <summary> A floating panel that shows a note source's description — and its image, if any — while one of its notes is hovered. </summary>
    /// <remarks> Built entirely in code, so it needs no scene file. Runs as a <see cref="Control.TopLevel"/> control positioned in global space, with mouse input ignored so it never interrupts the hover it is reacting to. </remarks>
    public partial class DescriptionTooltip : PanelContainer
    {
        /// <summary> The offset from the cursor at which the tooltip's top-left corner is placed. </summary>
        private static readonly Vector2 CursorOffset = new Vector2(16f, 16f);

        /// <summary> The tooltip's preferred width; its height grows to fit the wrapped content. </summary>
        private const Single Width = 280f;


        /// <summary> Renders the source's image, when it has one. </summary>
        private readonly TextureRect _image = new TextureRect();

        /// <summary> Renders the source's description as BBCode. </summary>
        private readonly RichTextLabel _label = new RichTextLabel();


        /// <inheritdoc/>
        public override void _Ready()
        {
            TopLevel    = true;                      // Position in global space, free of the parent's layout.
            Visible     = false;
            MouseFilter = MouseFilterEnum.Ignore;    // Never eat the hover or click that summoned us.

            _image.Visible      = false;
            _image.ExpandMode   = TextureRect.ExpandModeEnum.FitWidthProportional;
            _image.StretchMode  = TextureRect.StretchModeEnum.KeepAspectCentered;

            _label.BbcodeEnabled     = true;
            _label.FitContent        = true;
            _label.AutowrapMode      = TextServer.AutowrapMode.Word;
            _label.CustomMinimumSize = new Vector2(Width, 0f);

            VBoxContainer box = new VBoxContainer();
            box.AddChild(_image);
            box.AddChild(_label);
            AddChild(box);
        }


        /// <summary> Populates the tooltip from a note source and shows it anchored near the cursor. </summary>
        /// <param name="source"> The live source whose description and image to display. </param>
        /// <param name="cursor"> The current global cursor position to anchor against. </param>
        public void ShowFor(INoteSource source, Vector2 cursor)
        {
            _label.Text = DescriptionRenderer.ToBbcode(source.BuildDescription());

            Texture2D? texture = source.GetTexture();
            _image.Texture     = texture;
            _image.Visible     = texture is not null;

            GlobalPosition = cursor + CursorOffset;
            Visible        = true;
        }


        /// <summary> Hides the tooltip. </summary>
        public void HideTooltip() => Visible = false;
    }
}
