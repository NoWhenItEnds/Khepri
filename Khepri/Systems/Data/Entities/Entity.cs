using System;
using Godot;

namespace Khepri.Data.Entities
{
    /// <summary> An entity that exists within the game world. </summary>
    /// <remarks> The abstract data model, rather than the actual node, to allow for simulation even if they're not being rendered. </remarks>
    public abstract class Entity
    {
        /// <summary> The mass of the entity, in the simulation's own units. Drives how strongly it pulls on everything else; a massless body (zero) still feels gravity but exerts none. </summary>
        public Single Mass { get; set; }

        /// <summary> The physical radius of the entity, in world units. Drives how large its sprite is drawn; kept separate from mass so a small dense body and a large diffuse one can each be sized honestly. </summary>
        public Single Radius { get; set; }

        /// <summary> The entity's position within its solar system, in world units. </summary>
        public Vector2 Position { get; set; }

        /// <summary> The entity's velocity, in world units per second. </summary>
        public Vector2 Velocity { get; set; }

        /// <summary> The gravitational acceleration currently acting on the entity. Owned by the solar system's integrator, which recomputes it every step; nothing else should write to it. Thruster forces are kept separate in <see cref="AppliedForce"/> so this can be carried between steps. </summary>
        public Vector2 Acceleration { get; set; }


        /// <summary> The entity's facing, in radians. Zero points along positive X; thrust is applied along this heading. </summary>
        public Single Rotation { get; set; }

        /// <summary> The rate the entity is turning this step, in radians per second. Arcade-style: a controller sets it directly each frame and the integrator clears it, so a ship turns only while actively steering and stops the instant it lets go — there is no angular momentum. </summary>
        public Single AngularVelocity { get; set; }


        /// <summary> The linear force applied to the entity this step, in mass-units times world-units per second squared. Accumulated by <see cref="ApplyForce"/> and cleared once the integrator has consumed it. </summary>
        public Vector2 AppliedForce { get; set; }


        /// <summary> The Godot-position of the entity. Used to position the node correctly in the world. </summary>
        public Vector2 GlobalPosition => Position;


        /// <summary> Apply a linear force to the entity for the coming step. Forces accumulate, then the integrator consumes and clears them, so a controller reapplies force every physics frame it wants to keep thrusting. </summary>
        /// <param name="force"> The force to add, in world space. </param>
        public void ApplyForce(Vector2 force)
        {
            AppliedForce += force;
        }


        /// <summary> Turn the entity for the coming step at the given rate, in radians per second. Like <see cref="ApplyForce"/> this lasts a single step, so a controller reasserts it each physics frame it wants to keep turning; releasing it stops the turn at once. </summary>
        /// <param name="angularVelocity"> The turn rate to add; positive and negative turn opposite ways. </param>
        public void ApplyTurn(Single angularVelocity)
        {
            AngularVelocity += angularVelocity;
        }


        /// <summary> The system the entity is currently within. A null indicates that it's lost in null-space. The void between the stars. </summary>
        //private SolarSystem? _solarSystem = null;
    }
}
