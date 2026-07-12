using System;
using Godot;

namespace Khepri.World
{
    /// <summary> A snapshot of what a controller wants its entity to do this step. </summary>
    public readonly struct ActionIntent
    {
        /// <summary> How hard to burn along the entity's heading, from zero to one. </summary>
        public Single Throttle { get; }

        /// <summary> Which way to turn, from minus one to plus one. </summary>
        public Single Steer { get; }


        /// <summary> Create an intent, clamping each channel into its legal range. </summary>
        /// <param name="throttle"> How hard to burn, clamped to [0, 1]. </param>
        /// <param name="steer"> Which way to turn, clamped to [-1, 1]. </param>
        public ActionIntent(Single throttle, Single steer)  // TODO - Extend into other kinds of intent.
        {
            Throttle = Mathf.Clamp(throttle, 0f, 1f);
            Steer = Mathf.Clamp(steer, -1f, 1f);
        }
    }
}
