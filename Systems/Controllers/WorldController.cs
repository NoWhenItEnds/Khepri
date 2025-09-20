using Godot;
using Khepri.Nodes.Singletons;
using System;

namespace Khepri.Controllers
{
    /// <summary> Controls the world's state such as its time and seasons. </summary>
    public partial class WorldController : SingletonNode<WorldController>
    {
        /// <summary> The scaling applied to the game world time. Modifies it from the real-world time by the given factor. </summary>
        [ExportGroup("Settings")]
        [Export] private Single _timescale = 24f;


        /// <summary> The world's current time. </summary>
        /// <remarks> The game starts on Sunday the 5th of February, 2012. </remarks>
        public DateTimeOffset CurrentTime { get; private set; } = new DateTimeOffset(2012, 2, 5, 0, 0, 0, TimeSpan.FromHours(8));


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Update the world time each frame.
            CurrentTime += TimeSpan.FromSeconds(delta * _timescale);
        }
    }
}
