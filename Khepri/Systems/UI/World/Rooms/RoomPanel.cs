using System;
using System.Collections.Generic;
using Godot;
using Khepri.Descriptions;
using Khepri.Entities;
using Khepri.Rooms;

namespace Khepri.UI.World.Rooms
{
    /// <summary> The panel displaying a description about the current room the player inhabits. </summary>
    public partial class RoomPanel : Control
    {
        /// <summary> The label to show a description of the currently selected room. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _roomLabel = null!;


        /// <summary> Maps the meta key embedded in each rendered note to the live source backing its tooltip. Rebuilt on every <see cref="ForceUpdate"/>. </summary>
        private readonly Dictionary<String, INoteSource> _notes = new Dictionary<String, INoteSource>();

        /// <summary> The floating panel shown while a note is hovered. </summary>
        private DescriptionTooltip _tooltip = null!;

        /// <summary> The BBCode last applied to the label, so a per-frame <see cref="ForceUpdate"/> can skip re-rendering identical text — which would otherwise reset the label's hover state and dismiss the tooltip every frame. </summary>
        private String _rendered = String.Empty;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _roomLabel.BbcodeEnabled     =  true;
            _roomLabel.MetaHoverStarted  += OnNoteHoverStarted;
            _roomLabel.MetaHoverEnded    += OnNoteHoverEnded;

            // Parent the tooltip to the outermost Control rather than this panel, so it is neither
            // clipped by the tab container nor drawn under sibling panels, while sharing the label's
            // coordinate space for accurate cursor positioning. Deferred because the ancestry is still
            // building its children during _Ready; added last, it draws over its siblings.
            _tooltip = new DescriptionTooltip();
            GetOverlayRoot().CallDeferred(Node.MethodName.AddChild, _tooltip);
        }


        /// <summary> Walks up to the outermost <see cref="Control"/> ancestor, used as a full-window overlay parent for the tooltip. </summary>
        /// <returns> The topmost Control in this panel's ancestry. </returns>
        private Control GetOverlayRoot()
        {
            Control root = this;
            while (root.GetParent() is Control parent)
            {
                root = parent;
            }
            return root;
        }


        /// <summary> Force the UI element to reflect the current game's state. </summary>
        /// <param name="playerEntity"> The currently controlled player entity. </param>
        /// <param name="playerRoom"> The room the player currently inhabits. </param>
        public void ForceUpdate(Entity playerEntity, Room playerRoom)
        {
            String incoming = DescriptionRenderer.ToBbcode(playerRoom.BuildDescription(), _notes);

            // If the incoming is different from what we're currently showing.
            if(incoming != _rendered)
            {
                _rendered = incoming;
                _roomLabel.Text = incoming;
                _tooltip.HideTooltip();
            }
        }


        /// <summary> Shows the tooltip for a hovered note, resolving its meta key back to the source that backs it. </summary>
        /// <param name="meta"> The <c>[url]</c> meta of the hovered note, as recorded during rendering. </param>
        private void OnNoteHoverStarted(Variant meta)
        {
            if (_notes.TryGetValue(meta.AsString(), out INoteSource? source))
            {
                _tooltip.ShowFor(source, _roomLabel.GetGlobalMousePosition());
            }
        }


        /// <summary> Hides the tooltip once the cursor leaves a note. </summary>
        /// <param name="meta"> The <c>[url]</c> meta of the note being left; unused. </param>
        private void OnNoteHoverEnded(Variant meta) => _tooltip.HideTooltip();
    }
}
