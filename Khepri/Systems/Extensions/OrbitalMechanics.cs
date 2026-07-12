using System;
using Godot;
using Khepri.World.Entities;

namespace Khepri.Extensions
{
    /// <summary> The shared gravitational maths used by the simulation. </summary>
    public static class OrbitalMechanics
    {
        /// <summary> The gravitational constant used by the simulation. </summary>
        public const Single GravitationalConstant = 1f;

        /// <summary> A small softening distance, in world units, folded into every separation so gravity can't rocket toward infinity during a close encounter. </summary>
        public const Single Softening = 1f;


        /// <summary> Compute the softened gravitational acceleration a point mass induces across a separation. A zero separation contributes nothing — the softening absorbs it — so a body can safely be measured against itself. </summary>
        /// <param name="offset"> The separation from the accelerated body to the pulling mass, in world units. </param>
        /// <param name="mass"> The pulling body's mass. </param>
        /// <returns> The acceleration induced on the body at the separation's origin. </returns>
        public static Vector2 PointMassAcceleration(Vector2 offset, Single mass)
        {
            Single distanceSquared = offset.LengthSquared() + (Softening * Softening);
            Single falloff = GravitationalConstant * mass / (distanceSquared * Mathf.Sqrt(distanceSquared));

            return offset * falloff;
        }


        /// <summary> The mean motion, in radians per second, of an orbit of the given semi-major axis around the given mass: the rate its mean anomaly advances, and for a circle its plain angular velocity. </summary>
        /// <param name="primaryMass"> The mass of the body being orbited. </param>
        /// <param name="semiMajorAxis"> The orbit's semi-major axis, in world units. </param>
        public static Single MeanMotion(Single primaryMass, Single semiMajorAxis)
        {
            return Mathf.Sqrt(GravitationalConstant * primaryMass / (semiMajorAxis * semiMajorAxis * semiMajorAxis));
        }


        /// <summary> Compute the velocity that holds an orbiter on a circular orbit around a primary, relative to the primary itself. The single place the gravitational constant enters spawn maths, so authored orbits stay circular if it is ever retuned. </summary>
        /// <param name="primary"> The body being orbited, assumed to dominate the pair. </param>
        /// <param name="position"> The orbiter's position, in world units; must not coincide with the primary's. </param>
        /// <returns> The orbital velocity, relative to the primary — add the primary's own velocity for the world-space value. </returns>
        public static Vector2 CircularOrbitVelocity(Entity primary, Vector2 position)
        {
            // A circular orbit needs speed sqrt(G*M/r) aimed perpendicular to the radius. The handedness
            // of Orthogonal doesn't matter — either direction of travel traces the same stable circle.
            Vector2 offset = position - primary.Position;
            Single speed = Mathf.Sqrt(GravitationalConstant * primary.Mass / offset.Length());

            return offset.Orthogonal().Normalized() * speed;
        }
    }
}
