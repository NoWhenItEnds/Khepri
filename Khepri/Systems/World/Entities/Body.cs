using System;
using Godot;

namespace Khepri.World.Entities
{
    /// <summary> A celestial body — a star, planet, or moon. Celestials are the system's gravity sources. An anchored body holds its authored position; an orbiting one is carried by the <see cref="Entity.Orbit"/> it is given, whether an analytic rail or an integrated fall. </summary>
    public class Body : Entity
    {
        /// <summary> Create a body at a fixed position. An anchored body — a system's star — keeps this position; an orbiting one is re-placed by its orbit the moment it is added to a system, so the position here is just a sane value in the meantime. </summary>
        /// <param name="mass"> The body's mass, in the simulation's own units. </param>
        /// <param name="radius"> The body's physical radius, in world units. </param>
        /// <param name="position"> The body's starting position, in world units. </param>
        public Body(Single mass, Single radius, Vector2 position)
            : base(mass, radius, position, Vector2.Zero)
        {
        }
    }
}
