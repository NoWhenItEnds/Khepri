using Aphelion.Types.Extensions;
using Godot;
using Khepri.Nodes.Singletons;
using System;

namespace Khepri.Controllers
{
    /// <summary> Controls the world's state such as its time and seasons. </summary>
    public partial class WorldController : SingletonNode<WorldController>
    {
        /// <summary> The observer's latitude. </summary>
        [ExportGroup("Settings")]
        [Export] public Double Latitude { get; private set; } = -31.80529;

        /// <summary> The observer's longitude. </summary>
        [Export] public Double Longitude { get; private set; } = 115.74419;

        /// <summary> How quickly time is moving. </summary>
        /// <remarks> One in game day takes two hours. </remarks>
        [Export] private Single _timescale = 12f;

        /// <summary> How many degrees each pfile of star in the array represents. </summary>
        [Export] private Single _starSegmentSize = 2f;


        /// <summary> The world's current time. </summary>
        /// <remarks> The game starts on Sunday the 5th of February, 2012. </remarks>
        public DateTimeOffset CurrentTime { get; private set; } = new DateTimeOffset(2012, 2, 5, 0, 0, 0, TimeSpan.FromHours(8));

        /// <summary> The current local sidereal time relative to the observer's longitude. </summary>
        public Double LocalSiderealTime => CurrentTime.ToLocalSiderealTime(Longitude);

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
