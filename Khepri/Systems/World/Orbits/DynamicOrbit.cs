using System;
using Godot;
using Khepri.World.Entities;

namespace Khepri.World.Orbits
{
    /// <summary> An orbit that is integrated through the live gravity field rather than placed on a rail, so the body genuinely falls under every source's pull and can be perturbed, captured, or flung. Uses velocity Verlet in kick-drift-kick form, which is symplectic and so keeps orbits stable over long play sessions; the acceleration left at the end of a step is exactly what the next step's opening kick needs, so a single gravity sample per step suffices once primed. </summary>
    public sealed class DynamicOrbit : Orbit
    {
        /// <summary> Create a dynamic orbit for an entity, integrated from its current motion state. </summary>
        /// <param name="entity"> The entity this orbit governs. </param>
        public DynamicOrbit(Entity entity)
            : base(entity)
        {
        }


        /// <inheritdoc/>
        internal override void Prime(SolarSystem system)
        {
            Entity.Acceleration = system.GravityAt(Entity.Position);
        }


        /// <inheritdoc/>
        internal override void KickStart(Single delta)
        {
            Entity.Velocity += TotalAcceleration() * (delta * 0.5f);
        }


        /// <inheritdoc/>
        internal override void Advance(Double time, Single delta)
        {
            Entity.Position += Entity.Velocity * delta;
            Entity.Rotation += Entity.AngularVelocity * delta;
        }


        /// <inheritdoc/>
        internal override void KickEnd(SolarSystem system, Single delta)
        {
            // The sources have moved to their new positions, so the recomputed gravity matches where they
            // actually are before the closing kick carries the entity's velocity the rest of the step.
            Entity.Acceleration = system.GravityAt(Entity.Position);
            Entity.Velocity += TotalAcceleration() * (delta * 0.5f);
        }


        /// <summary> The acceleration driving each half-kick: the carried gravity plus this step's thruster force. A massless entity can't be pushed, so its thruster contribution collapses to nothing rather than dividing by zero. </summary>
        private Vector2 TotalAcceleration()
        {
            Vector2 thrust = Entity.Mass > 0f ? Entity.AppliedForce / Entity.Mass : Vector2.Zero;

            return Entity.Acceleration + thrust;
        }
    }
}
