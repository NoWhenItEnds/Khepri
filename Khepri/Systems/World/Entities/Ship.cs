using System;
using Godot;

namespace Khepri.World.Entities
{
    /// <summary> A powered, steerable vessel — the player's craft or an AI's — that coasts under gravity like any dynamic entity but can also burn along its heading and turn under a controller's command. </summary>
    public class Ship : Entity
    {
        /// <summary> The force the ship's engine produces at full throttle, in the simulation's force units. </summary>
        public Single ThrustPower { get; private set; }

        /// <summary> How quickly the ship turns at full steering, in radians per second. Arcade-style: it reaches this rate at once and stops the moment steering releases. </summary>
        public Single TurnSpeed { get; private set; }


        /// <summary> Create a ship with its engine, handling, and a starting motion state. </summary>
        /// <param name="mass"> The ship's mass, in the simulation's own units. </param>
        /// <param name="radius"> The ship's physical radius, in world units. </param>
        /// <param name="thrustPower"> The force the engine produces at full throttle. </param>
        /// <param name="turnSpeed"> How quickly the ship turns at full steering, in radians per second. </param>
        /// <param name="position"> The ship's starting position, in world units. </param>
        /// <param name="velocity"> The ship's starting velocity, in world units per second. </param>
        public Ship(Single mass, Single radius, Single thrustPower, Single turnSpeed, Vector2 position, Vector2 velocity)
            : base(mass, radius, position, velocity)
        {
            ThrustPower = thrustPower;
            TurnSpeed = turnSpeed;
        }


        public void HandleInput(ActionIntent intent)
        {
            Thrust(intent.Throttle);
            Steer(intent.Steer);
        }


        /// <summary> Fire the engine along the ship's current heading for the coming step. Private so control only flows through the intent seam: a controller expresses throttle in its <see cref="ActionIntent"/>, never by reaching in and firing the engine itself. </summary>
        /// <param name="throttle"> How hard to burn, from zero to one. </param>
        private void Thrust(Single throttle)
        {
            Vector2 heading = Vector2.Right.Rotated(Rotation);
            ApplyForce(heading * (ThrustPower * throttle));
        }


        /// <summary> Steer the ship for the coming step. Private so control only flows through the intent seam, like <see cref="Thrust"/>. </summary>
        /// <param name="direction"> Which way to turn, from minus one to plus one. </param>
        private void Steer(Single direction)
        {
            AngularVelocity += TurnSpeed * direction;
        }
    }
}
