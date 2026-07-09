using System;
using Godot;

namespace Khepri.Data.Entities
{
    /// <summary> A simple physical body — a star, planet, moon, or drifting rock — that moves purely under the gravity of everything around it. </summary>
    public class Body : Entity
    {
        /// <summary> Create a body with a starting mass, size, and state. </summary>
        /// <param name="mass"> The body's mass, driving how strongly it pulls on everything else. </param>
        /// <param name="radius"> The body's physical radius, in world units, driving how large it is drawn. </param>
        /// <param name="position"> The body's starting position, in world units. </param>
        /// <param name="velocity"> The body's starting velocity, in world units per second. </param>
        public Body(Single mass, Single radius, Vector2 position, Vector2 velocity)
        {
            Mass = mass;
            Radius = radius;
            Position = position;
            Velocity = velocity;
        }
    }
}
