using System;
using Godot;
using Jaypen.Singletons;
using Khepri.Data.Entities;
using Khepri.Managers;

namespace Khepri.Controllers
{
    /// <summary> Spawns the player's ship and flies it from the keyboard, turning key presses into thrust and steering each physics frame before the world integrates them. </summary>
    public partial class PlayerController : SingletonNode<PlayerController>
    {
        /// <summary> The mass of the player's ship. </summary>
        [ExportGroup("Ship")]
        [Export] private Single _mass = 1f;

        /// <summary> The physical radius of the player's ship, in world units. </summary>
        [Export] private Single _radius = 12f;

        /// <summary> The force the ship's engine produces at full throttle. </summary>
        [Export] private Single _thrustPower = 8f;

        /// <summary> The ship's turn rate at full steering, in radians per simulation-second. The world timescale multiplies this in real terms, so keep it small. </summary>
        [Export] private Single _turnSpeed = 0.25f;

        /// <summary> The radius of the circular orbit the ship starts on, around the star at the origin. </summary>
        [ExportGroup("Spawn")]
        [Export] private Single _spawnRadius = 250f;


        /// <summary> The mass of the central star the ship's opening orbit is sized against. Matches the star seeded by the game manager, so the ship begins on a stable circle rather than falling straight in. </summary>
        private const Single CentralMass = 1_000_000f;


        /// <summary> The player's ship within the simulation, or null before it has been spawned. Exposed so the camera can follow it. </summary>
        public Ship? PlayerShip { get; private set; }


        /// <inheritdoc/>
        public override void _Ready()
        {
            Vector2 position = Vector2.Right * _spawnRadius;
            Vector2 velocity = position.Orthogonal().Normalized() * Mathf.Sqrt(CentralMass / _spawnRadius);

            PlayerShip = new Ship(_mass, _radius, _thrustPower, _turnSpeed, position, velocity);
            EntityManager.Instance?.AddEntity(PlayerShip);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Reads physical key positions rather than a mapped action, so it works before any input map
            // is set up and stays correct on non-QWERTY layouts. W burns along the heading; A and D steer.
            if (PlayerShip != null)
            {
                Single throttle = Input.IsPhysicalKeyPressed(Key.W) ? 1f : 0f;
                Single steer = (Input.IsPhysicalKeyPressed(Key.D) ? 1f : 0f) - (Input.IsPhysicalKeyPressed(Key.A) ? 1f : 0f);

                PlayerShip.Thrust(throttle);
                PlayerShip.Steer(steer);
            }
        }
    }
}
