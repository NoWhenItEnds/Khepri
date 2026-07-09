using System;
using System.Collections.Generic;
using Godot;
using Khepri.Data.Entities;

namespace Khepri.Data
{
    /// <summary> A single solar system within the larger galaxy. </summary>
    public class SolarSystem
    {
        /// <summary> The gravitational constant used by the simulation. Tune this to taste rather than using the real-world value; it trades directly against the mass and distance scales you author. </summary>
        private const Single GravitationalConstant = 1f;

        /// <summary> A small softening distance, in world units, folded into every separation so gravity can't rocket toward infinity during a close encounter. </summary>
        private const Single Softening = 1f;


        /// <summary> All the entities that currently exist within the system. </summary>
        private HashSet<Entity> _entities = new HashSet<Entity>();

        /// <summary> Whether every entity's acceleration currently reflects the live gravity field, letting the integrator carry it into the next step instead of recomputing it up front. Cleared whenever the population changes. </summary>
        private Boolean _primed = false;


        /// <summary> Begin simulating an entity as part of this system, pulling on and being pulled by every other body within it. </summary>
        /// <param name="entity"> The entity to add to the system. </param>
        public void Add(Entity entity)
        {
            // A new body changes the pull felt by every other, so the carried accelerations are stale;
            // dropping the primed flag forces a fresh gravity pass at the start of the next step.
            _entities.Add(entity);
            _primed = false;
        }


        /// <summary> Get every entity currently being simulated within the system. </summary>
        /// <returns> The system's entities, for the renderer to draw and the view to cull against. </returns>
        public IEnumerable<Entity> GetEntities() => _entities;


        /// <summary> Advance the whole system forward by a single fixed timestep using velocity Verlet integration. </summary>
        /// <param name="delta"> The fixed timestep, in seconds, to advance the simulation by. </param>
        public void Step(Single delta)
        {
            // Velocity Verlet, in its kick-drift-kick form, which integrates every body symplectically
            // and so keeps orbits stable over long play sessions rather than spiralling in or winding out.
            // The acceleration left at the end of a step is exactly what the next step's opening kick needs
            // — positions don't move in between — so it's carried over, leaving a single gravity pass per
            // step. Only the very first step, or the first after the population changes, has to prime it.
            if (!_primed)
            {
                AccumulateAccelerations();
                _primed = true;
            }

            ApplyHalfKick(delta);
            ApplyDrift(delta);
            AccumulateAccelerations();
            ApplyHalfKick(delta);
            ClearAppliedInputs();
        }


        /// <summary> Recompute the gravitational acceleration acting on every entity from the pull of every other entity. </summary>
        private void AccumulateAccelerations()
        {
            foreach (Entity entity in _entities)
            {
                Vector2 acceleration = Vector2.Zero;

                foreach (Entity other in _entities)
                {
                    // A body exerts no pull on itself: when other == entity the offset is zero, so the
                    // term below contributes nothing on its own and needs no explicit self-check.
                    Vector2 offset = other.Position - entity.Position;
                    Single distanceSquared = offset.LengthSquared() + (Softening * Softening);
                    Single falloff = GravitationalConstant * other.Mass / (distanceSquared * Mathf.Sqrt(distanceSquared));
                    acceleration += offset * falloff;
                }

                entity.Acceleration = acceleration;
            }
        }


        /// <summary> Apply half a timestep to every entity's velocity, from both gravity and this frame's thruster force. </summary>
        private void ApplyHalfKick(Single delta)
        {
            foreach (Entity entity in _entities)
            {
                // Gravity is carried in Acceleration; thruster force is turned into acceleration live from
                // this frame's input. Both hold constant across the step, so halving the kick around the
                // drift keeps the integration symplectic. A massless body can't be pushed, so its thruster
                // contribution collapses to nothing rather than dividing by zero.
                Vector2 thrustAcceleration = entity.Mass > 0f ? entity.AppliedForce / entity.Mass : Vector2.Zero;

                entity.Velocity += (entity.Acceleration + thrustAcceleration) * (delta * 0.5f);
            }
        }


        /// <summary> Advance every entity's position and facing by a full timestep of its current linear and angular velocity. </summary>
        private void ApplyDrift(Single delta)
        {
            foreach (Entity entity in _entities)
            {
                entity.Position += entity.Velocity * delta;
                entity.Rotation += entity.AngularVelocity * delta;
            }
        }


        /// <summary> Clear the per-step thruster force and turn from every entity now the integrator has consumed them, so both last only while a controller keeps reasserting them. </summary>
        private void ClearAppliedInputs()
        {
            foreach (Entity entity in _entities)
            {
                entity.AppliedForce = Vector2.Zero;
                entity.AngularVelocity = 0f;
            }
        }
    }
}
