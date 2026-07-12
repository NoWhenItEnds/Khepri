using System;
using Godot;
using Khepri.World.Orbits;

namespace Khepri.World.Entities
{
    /// <summary> An entity that exists within the game world. </summary>
    /// <remarks> The abstract data model, rather than the actual node, to allow for simulation even if they're not being rendered. </remarks>
    public abstract class Entity
    {
        /// <summary> The mass of the entity, in the simulation's own units. Drives how strongly it pulls on everything else; a massless body (zero) still feels gravity but exerts none. </summary>
        public Single Mass { get; private set; }

        /// <summary> The physical radius of the entity, in world units. Drives how large its sprite is drawn; kept separate from mass so a small dense body and a large diffuse one can each be sized honestly. </summary>
        public Single Radius { get; private set; }

        /// <summary> The entity's position within its solar system, in world units. Owned by the simulation: rails place celestials, the integrator moves dynamics. </summary>
        public Vector2 Position { get; internal set; }

        /// <summary> The entity's velocity, in world units per second. Owned by the simulation, like <see cref="Position"/>. </summary>
        public Vector2 Velocity { get; internal set; }

        /// <summary> The gravitational acceleration carried between integrator steps. Owned by the integrator; thruster forces are kept separate in <see cref="AppliedForce"/> so this can be carried over. </summary>
        internal Vector2 Acceleration { get; set; }


        /// <summary> The entity's facing, in radians. Zero points along positive X. </summary>
        public Single Rotation { get; internal set; }

        /// <summary> The rate the entity is turning this step, in radians per second. Arcade-style: a controller sets it each frame and the integrator clears it, so an entity turns only while actively steered and stops the instant the input releases — there is no angular momentum. </summary>
        public Single AngularVelocity { get; internal set; }


        /// <summary> The linear force applied to the entity this step, in mass-units times world-units per second squared. Accumulated by <see cref="ApplyForce"/> and cleared once the integrator has consumed it. </summary>
        public Vector2 AppliedForce { get; private set; }


        /// <summary> The system the entity is currently within, set by the system when the entity is added. A null indicates that it's lost in null-space. The void between the stars. </summary>
        public SolarSystem? System { get; internal set; }

        /// <summary> How the entity is positioned each step — an analytic rail or an integrated fall — or null when it is anchored and never moves, like a system's star. </summary>
        public Orbit? Orbit { get; internal set; }


        /// <summary> Create an entity with its birth-time identity and motion state. Identity is settable only here; motion state is owned by the simulation layer afterwards, so everything outside it influences an entity through forces and intents, never by rewriting state directly. </summary>
        /// <param name="mass"> The entity's mass, in the simulation's own units. </param>
        /// <param name="radius"> The entity's physical radius, in world units. </param>
        /// <param name="position"> The entity's starting position, in world units. </param>
        /// <param name="velocity"> The entity's starting velocity, in world units per second. </param>
        protected Entity(Single mass, Single radius, Vector2 position, Vector2 velocity)
        {
            Mass = mass;
            Radius = radius;
            Position = position;
            Velocity = velocity;
        }


        /// <summary> Apply a linear force to the entity for the coming step. Forces accumulate, then the integrator consumes and clears them, so a controller reapplies force every physics frame it wants to keep thrusting. </summary>
        /// <param name="force"> The force to add, in world space. </param>
        public void ApplyForce(Vector2 force)
        {
            AppliedForce += force;
        }


        /// <summary> Gather this step's control inputs. Called by the solar system at the start of every step; the base entity is uncontrolled and does nothing. </summary>
        internal virtual void PollController()
        {
        }


        /// <summary> Clear the per-step force and turn inputs now the integrator has consumed them, so they last only while a controller keeps reasserting them. Internal because it belongs to the integrator. </summary>
        internal void ClearAppliedInputs()
        {
            AppliedForce = Vector2.Zero;
            AngularVelocity = 0f;
        }
    }
}
