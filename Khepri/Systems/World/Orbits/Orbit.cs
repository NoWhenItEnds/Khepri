using System;
using Khepri.World.Entities;

namespace Khepri.World.Orbits
{
    /// <summary> How an entity is positioned within its solar system each simulation step. A fixed orbit places the entity analytically; a dynamic orbit integrates it through the gravity field. The simulation drives every entity through this one seam, so it never has to know which kind it is stepping. </summary>
    public abstract class Orbit
    {
        /// <summary> The entity this orbit governs. Its motion state is written here and nowhere else. </summary>
        protected Entity Entity { get; }


        /// <summary> Create an orbit bound to the entity it will move. </summary>
        /// <param name="entity"> The entity this orbit governs. </param>
        protected Orbit(Entity entity)
        {
            Entity = entity;
        }


        /// <summary> Seed whatever the orbit carries between steps and place the entity for the system's current time. Called once before stepping begins and again whenever the population changes. </summary>
        /// <param name="system"> The system the entity belongs to, sampled for its gravity field. </param>
        internal abstract void Prime(SolarSystem system);


        /// <summary> The opening half-kick of velocity Verlet, applied before the drift. Kinematic orbits are placed by their rail and ignore it. </summary>
        /// <param name="delta"> The timestep, in seconds. </param>
        internal virtual void KickStart(Single delta)
        {
        }


        /// <summary> Advance the entity to the new time: a rail jumps to its exact analytic point, an integrated orbit drifts by its velocity. </summary>
        /// <param name="time"> The system's simulated time after this step, in seconds since the epoch. </param>
        /// <param name="delta"> The timestep, in seconds. </param>
        internal abstract void Advance(Double time, Single delta);


        /// <summary> The closing half-kick of velocity Verlet, applied after every source has been repositioned. Kinematic orbits ignore it. </summary>
        /// <param name="system"> The system the entity belongs to, resampled for its gravity field. </param>
        /// <param name="delta"> The timestep, in seconds. </param>
        internal virtual void KickEnd(SolarSystem system, Single delta)
        {
        }
    }
}
