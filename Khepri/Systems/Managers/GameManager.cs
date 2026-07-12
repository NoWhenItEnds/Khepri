using System;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Controllers;
using Khepri.Extensions;
using Khepri.World;
using Khepri.World.Entities;
using Khepri.World.Orbits;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            SolarSystemManager? systemManager = SolarSystemManager.Instance;

            if (systemManager != null)
            {
                SolarSystem system = systemManager.AddSystem();
                BuildStartingSystem(system);
                SpawnShip(system);
            }
        }


        /// <summary> Seed the opening system with a star and a few bodies. Test content for now — a save file or procedural generation will build this in the future — and hand-authored inline now that the resource layer is gone. </summary>
        /// <param name="system"> The freshly created system to populate. </param>
        private static void BuildStartingSystem(SolarSystem system)
        {
            Body star = new Body(100000f, 64f, Vector2.Zero);
            system.AddCelestial(star);

            Body planet = new Body(800f, 20f, Vector2.Zero);
            planet.Orbit = new StaticOrbit(planet, star, 500f, 0.25f, 0f, 0f);
            system.AddCelestial(planet);

            Body moon = new Body(20f, 6f, Vector2.Zero);
            moon.Orbit = new StaticOrbit(moon, planet, 70f, 0.1f, 0f, 1.2f);
            system.AddCelestial(moon);

            Body outer = new Body(500f, 16f, Vector2.Zero);
            outer.Orbit = new StaticOrbit(outer, star, 850f, 0.4f, Mathf.Pi * 0.5f, 2f);
            system.AddCelestial(outer);
        }


        /// <summary> Spawn the player's ship on a circular orbit around the system's primary body. Called by the game manager once the system has been seeded, so the primary genuinely exists before the ship tries to orbit it. A system with no primary gets no ship — there is nothing to orbit. </summary>
        /// <param name="system"> The system to spawn the ship in. </param>
        private static void SpawnShip(SolarSystem system)
        {
            Body? primary = system.Primary;

            if (primary != null)
            {
                Vector2 position = primary.Position + Vector2.Right * 100f;
                Vector2 velocity = primary.Velocity + OrbitalMechanics.CircularOrbitVelocity(primary, position);

                Ship ship = new Ship(1f, 8f, 200f, 3f, position, velocity);
                ship.Orbit = new DynamicOrbit(ship);

                system.AddDynamic(ship);

                ControllerManager.Instance?.PlayerController.SetEntity(ship);
            }
        }
    }
}
