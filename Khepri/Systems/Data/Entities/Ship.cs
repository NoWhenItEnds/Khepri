using System;
using Godot;

namespace Khepri.Data.Entities
{
    /// <summary> A powered, steerable vessel — the player's craft or an AI's — that coasts under gravity like any body but can also burn along its heading and turn under a controller's command. </summary>
    public class Ship : Entity
    {
        /// <summary> The force the ship's engine produces at full throttle, in the simulation's force units. </summary>
        public Single ThrustPower { get; set; }

        /// <summary> How quickly the ship turns at full steering, in radians per second. Arcade-style: it reaches this rate at once and stops the moment steering releases. </summary>
        public Single TurnSpeed { get; set; }


        /// <summary> Create a ship with a starting mass, size, engine and handling, and motion state. </summary>
        /// <param name="mass"> The ship's mass, driving both its gravity and how sluggishly it responds to thrust. </param>
        /// <param name="radius"> The ship's physical radius, in world units, driving how large it is drawn. </param>
        /// <param name="thrustPower"> The force the engine produces at full throttle. </param>
        /// <param name="turnSpeed"> The turn rate at full steering, in radians per second. </param>
        /// <param name="position"> The ship's starting position, in world units. </param>
        /// <param name="velocity"> The ship's starting velocity, in world units per second. </param>
        public Ship(Single mass, Single radius, Single thrustPower, Single turnSpeed, Vector2 position, Vector2 velocity)
        {
            Mass = mass;
            Radius = radius;
            ThrustPower = thrustPower;
            TurnSpeed = turnSpeed;
            Position = position;
            Velocity = velocity;
        }


        /// <summary> Fire the engine along the ship's current heading for the coming step. </summary>
        /// <param name="throttle"> How hard to burn, from zero to one. </param>
        public void Thrust(Single throttle)
        {
            Vector2 heading = Vector2.Right.Rotated(Rotation);
            ApplyForce(heading * (ThrustPower * throttle));
        }


        /// <summary> Steer the ship for the coming step. </summary>
        /// <param name="direction"> Which way to turn, from minus one to plus one. </param>
        public void Steer(Single direction)
        {
            ApplyTurn(TurnSpeed * direction);
        }
    }
}
