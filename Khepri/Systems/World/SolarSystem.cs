using System;
using System.Collections.Generic;
using Godot;
using Khepri.Extensions;
using Khepri.World.Entities;
using Khepri.World.Orbits;

namespace Khepri.World
{
    /// <summary> A single solar system within the larger galaxy, and a self-contained simulation: it owns its own clock and its whole population. Every entity is moved through its <see cref="Entity.Orbit"/> — an analytic rail exact at any timescale, or an integrated fall through the gravity field — so the system itself never has to know which kind it is stepping. </summary>
    public class SolarSystem
    {
        /// <summary> Every entity in the system, in the order they were added — primaries before their satellites, so a rail update always reads a current parent position. Stepped and rendered as one population. </summary>
        private readonly List<Entity> _bodies = new List<Entity>();

        /// <summary> The gravity sources: the celestial bodies whose mass everything else falls toward. A subset of <see cref="_bodies"/>, kept apart so the field is summed over only the bodies that actually pull. </summary>
        private readonly List<Body> _sources = new List<Body>();


        /// <summary> The system's dominant body — the anchored celestial everything ultimately orbits, claimed by the first anchored body added. Null while the system has no anchored celestial. </summary>
        public Body? Primary { get; private set; }


        /// <summary> The simulated seconds elapsed since the epoch. Railed positions are pure functions of this clock, so the system can be evaluated at any moment without stepping through the moments in between. Written by <see cref="Step"/>; also restored directly by the save loader. </summary>
        public Double Time { get; internal set; } = 0.0;


        /// <summary> The gravity-sourcing celestial bodies, exposed read-only for future save and UI code. </summary>
        public IReadOnlyList<Body> Celestials => _sources;


        /// <summary> Whether every integrated entity's carried acceleration reflects the live gravity field, letting the integrator carry it into the next step instead of recomputing it up front. Cleared whenever the population changes. </summary>
        private Boolean _primed = false;


        /// <summary> Add a celestial body — a gravity source. It may be anchored (a null orbit), railed, or integrated, but if it rides a rail its primary must already be in the system. </summary>
        /// <param name="body"> The body to add. </param>
        /// <exception cref="ArgumentException"> Thrown when the body rides a rail around a primary that has not been added yet — rails are evaluated in insertion order, so a satellite ahead of its primary would read a stale parent position. </exception>
        public void AddCelestial(Body body)
        {
            if (body.Orbit is StaticOrbit staticOrbit && !_sources.Contains(staticOrbit.Primary))
            {
                throw new ArgumentException("A celestial must be added after its primary, so rail updates always read a current parent position.", nameof(body));
            }

            _sources.Add(body);
            _bodies.Add(body);
            body.System = this;

            // The first anchored celestial is the system's primary: the star everything ultimately
            // orbits, and the body spawners size their opening orbits against.
            if (Primary == null && body.Orbit == null)
            {
                Primary = body;
            }

            // Place the body at the system's current time immediately, so it never renders at a stale
            // epoch position before the first step. A new source also changes the pull on everything
            // else, so the carried accelerations are stale.
            body.Orbit?.Prime(this);
            _primed = false;
        }


        /// <summary> Add a dynamic entity — integrated under the celestials' gravity, exerting none of its own. </summary>
        /// <param name="entity"> The entity to add. </param>
        public void AddDynamic(Entity entity)
        {
            _bodies.Add(entity);
            entity.System = this;
            _primed = false;
        }


        /// <summary> Get every entity within the system, in add order, for the renderer to draw and the view to cull against. </summary>
        public IEnumerable<Entity> GetEntities()
        {
            foreach (Entity entity in _bodies)
            {
                yield return entity;
            }
        }


        /// <summary> The summed gravitational acceleration the system's celestials induce at a position. </summary>
        /// <param name="position"> The position to sample the gravity field at, in world units. </param>
        public Vector2 GravityAt(Vector2 position)
        {
            Vector2 acceleration = Vector2.Zero;

            foreach (Body body in _sources)
            {
                acceleration += OrbitalMechanics.PointMassAcceleration(body.Position - position, body.Mass);
            }

            return acceleration;
        }


        /// <summary> Advance the whole system forward by the given amount of simulated time. </summary>
        /// <param name="delta"> The simulated time, in seconds, to advance by — usually the fixed physics tick already scaled by the world timescale. </param>
        /// <param name="maxStepDuration"> The longest simulated timestep, in seconds, a single integrator pass may cover. Longer deltas are split into equal substeps, so a high world timescale costs extra integrator passes rather than accuracy. Only the integrated bodies need this; rails are exact at any step size. </param>
        public void Step(Single delta, Single maxStepDuration)
        {
            // Controllers are polled exactly once per step, here at the top, so an intent acts across
            // the whole step however many substeps it splits into. The inputs it produced are cleared
            // only after the final substep for the same reason.
            foreach (Entity entity in _bodies)
            {
                entity.PollController();
            }

            // Split long steps so integration accuracy is bounded by the step cap rather than by the
            // world timescale. The cap is floored so a zeroed inspector value can't explode the count.
            Int32 substepCount = Math.Max(1, Mathf.CeilToInt(delta / Mathf.Max(maxStepDuration, 0.001f)));
            Single substepDelta = delta / substepCount;

            for (Int32 i = 0; i < substepCount; i++)
            {
                StepOnce(substepDelta);
            }

            foreach (Entity entity in _bodies)
            {
                entity.ClearAppliedInputs();
            }
        }


        /// <summary> Advance the system by a single timestep of velocity Verlet, in kick-drift-kick form. Rails place their bodies exactly at the new time; integrated bodies are half-kicked, drifted, then half-kicked again through the resulting gravity field. The acceleration left at the end of a step is exactly what the next step's opening kick needs, so only the first step — or the first after the population changes — has to prime it. </summary>
        /// <param name="delta"> The timestep, in seconds, to advance by. </param>
        private void StepOnce(Single delta)
        {
            if (!_primed)
            {
                foreach (Entity entity in _bodies)
                {
                    entity.Orbit?.Prime(this);
                }

                _primed = true;
            }

            foreach (Entity entity in _bodies)
            {
                entity.Orbit?.KickStart(delta);
            }

            // The clock advances before the drift so rails place their bodies at exactly the new time,
            // parents before satellites, before any integrated body samples the gravity they now source.
            Time += delta;

            foreach (Entity entity in _bodies)
            {
                entity.Orbit?.Advance(Time, delta);
            }

            foreach (Entity entity in _bodies)
            {
                entity.Orbit?.KickEnd(this, delta);
            }
        }
    }
}
