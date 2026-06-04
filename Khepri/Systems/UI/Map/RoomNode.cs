using Godot;
using Khepri.Rooms;
using System;

namespace Khepri.UI.Map
{
    /// <summary> A single room marker on the overworld node graph: a label-bearing control positioned and recycled by <see cref="OverworldPanel"/>. </summary>
    public partial class RoomNode : Control
    {
        /// <summary> The label showing the room's identifier. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _nameLabel = null!;


        /// <summary> Number of leading characters from the room's <see cref="Guid"/> shown as a short stable identifier. </summary>
        private const Int32 UIdPrefixLength = 8;

        /// <summary> The colour multiplier applied to the marker representing the player's current room. </summary>
        private static readonly Color CurrentRoomTint = new Color(0.2f, 0.6f, 1.0f, 1.0f);


        /// <inheritdoc/>
        public override void _Ready()
        {
            // The marker lives directly on the panel (a non-container parent), so it keeps whatever size it is
            // given. Mirror the prefab's minimum size into the actual size so CenterOn has real dimensions to
            // offset against.
            Size = CustomMinimumSize;
        }


        /// <summary> Updates the marker to represent <paramref name="room"/>, tinting it when it is the player's current room. </summary>
        /// <param name="room"> The room this marker represents. </param>
        /// <param name="isCurrent"> Whether <paramref name="room"/> is the room the player currently occupies. </param>
        public void SetRoom(Room room, Boolean isCurrent)
        {
            // TODO: Replace the UID prefix with a human-readable name once rooms carry one at runtime
            //       (e.g. via a NameFeature sourced from the prefab's ResourceName).
            String shortId = room.UId.ToString()[..UIdPrefixLength];
            Int32 exits = room.GetConnections().Count;

            Color colour = isCurrent ? CurrentRoomTint : Colors.White;
            _nameLabel.Text = $"[color={colour.ToHtml()}]{shortId}{System.Environment.NewLine}[/color]";
        }


        /// <summary> Positions the marker so its centre sits at <paramref name="point"/> in the parent's coordinate space. </summary>
        /// <param name="point"> The desired centre point for the marker. </param>
        public void CenterOn(Vector2 point) => Position = point - (Size / 2f);
    }
}
