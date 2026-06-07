using Godot;
using Khepri.Entities;
using Khepri.Managers;
using Khepri.Rooms;
using System;

namespace Khepri.UI.World
{
    /// <summary> The node that displays information about the current state of the world. </summary>
    /// <remarks> Information such as time, temperature, etc. Passive information that doesn't fit in the description of a room, but humans intuitively understand. </remarks>
    public partial class StatusBar : Control
    {
        /// <summary> The label to show the game's DateTime. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _dateTimeLabel = null!;

        /// <summary> The label to show the game's current weather. </summary>
        [Export] private RichTextLabel _weatherLabel = null!;   // TODO - Turn this into a reusable icon label. Use this for wind and temp labels.


        /// <summary> The format to use for displaying the status bar's date time. </summary>
        private const String DATETIME_FORMAT = "dddd, dd MMMM yyyy HH:mm";


        /// <summary> Force the UI element to reflect the current game's state. </summary>
        /// <param name="playerEntity"> The currently controlled player entity. </param>
        /// <param name="playerRoom"> The room the player currently inhabits. </param>
        public void ForceUpdate(Entity playerEntity, Room playerRoom)
        {
            _dateTimeLabel.Text = GameManager.Instance!.GameTime.ToString(DATETIME_FORMAT);
            // TODO - Add other states. Weather, temp, etc.
        }
    }
}
