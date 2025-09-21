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

        /// <summary> The amount of time (in seconds) that has passed in game time since the previous physics frame. </summary>
        public Double GameTimeDelta { get; private set; }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            DateTimeOffset previousTime = CurrentTime;
            CurrentTime = CurrentTime.AddSeconds(delta * _timescale);
            GameTimeDelta = (CurrentTime.ToUnixTimeMilliseconds() - previousTime.ToUnixTimeMilliseconds()) * 0.001f;
        }
    }
}
