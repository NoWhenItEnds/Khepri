using Godot;
using Khepri.Controllers;
using System;

namespace Khepri.UI.HUD
{
    /// <summary> A UI element to represent the world's current time and state. </summary>
    public partial class Astrolabe : Control
    {
        /// <summary> The node holding the numeral ring texture. </summary>
        [ExportGroup("Nodes")]
        [Export] private TextureRect _ringTexture;


        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;


        /// <summary> The number of seconds in a day. </summary>
        private const Double TOTAL_SECONDS_DAY = 86400.0;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _worldController = WorldController.Instance;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            SetTime(_worldController.CurrentTime);
        }


        /// <summary> Update the astrolabe to reflect the given time. </summary>
        /// <param name="time"> The time to set. </param>
        private void SetTime(DateTimeOffset time)
        {
            TimeSpan day = time - time.Date;    // Get the time that has progressed since midnight.
            Double dayProgress = Mathf.InverseLerp(0.0, TOTAL_SECONDS_DAY, day.TotalSeconds);
            Double rotation = Mathf.Lerp(0f, 360f, dayProgress);
            _ringTexture.RotationDegrees = (Single)rotation;
        }
    }
}
