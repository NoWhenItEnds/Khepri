using System;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Data.Entities;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's central manager. The entrypoint. Works a little like Program.cs. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> The game world's current timescale. </summary>
        [ExportGroup("Settings")]
        [Export] public Single Timescale { get; private set; } = 12f;


        /// <summary> The current, universal time across the entire galaxy. </summary>
        public DateTime UniversalTime { get; private set; } = DateTime.UtcNow;


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            Logger.LogInformation("Hello, World!");
            SeedSolarSystem();
        }


        /// <summary> Seed the world with a temporary demonstration system — a heavy central star circled by a few light planets — so the simulation has something to move and draw. </summary>
        private void SeedSolarSystem()
        {
            EntityManager? entities = EntityManager.Instance;

            if (entities != null)
            {
                Single starMass = 1_000_000f;

                entities.AddEntity(new Body(starMass, 90f, Vector2.Zero, Vector2.Zero));
                entities.AddEntity(OrbitingBody(starMass, 180f, 0f, 1f, 16f));
                entities.AddEntity(OrbitingBody(starMass, 300f, Mathf.Tau / 3f, 1f, 22f));
                entities.AddEntity(OrbitingBody(starMass, 420f, Mathf.Tau * 2f / 3f, 1f, 28f));
            }
        }


        /// <summary> Create a light body on a circular orbit around a central mass sitting at the origin. </summary>
        /// <param name="centralMass"> The mass of the body being orbited, assumed to dominate and sit at the origin. </param>
        /// <param name="radius"> The orbital radius, in world units. </param>
        /// <param name="angle"> The starting angle around the orbit, in radians. </param>
        /// <param name="mass"> The orbiting body's own mass. </param>
        /// <param name="size"> The orbiting body's physical radius, in world units, driving how large it is drawn. </param>
        /// <returns> A body positioned on the orbit and given the perpendicular speed that holds it there. </returns>
        private static Body OrbitingBody(Single centralMass, Single radius, Single angle, Single mass, Single size)
        {
            // A circular orbit needs speed sqrt(G*M/r) aimed perpendicular to the radius; the simulation's
            // gravitational constant is one, so it falls out here. The handedness of Orthogonal doesn't
            // matter — either direction of travel traces the same stable circle.
            Vector2 direction = Vector2.Right.Rotated(angle);
            Vector2 position = direction * radius;
            Vector2 velocity = direction.Orthogonal() * Mathf.Sqrt(centralMass / radius);

            return new Body(mass, size, position, velocity);
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            UniversalTime += TimeSpan.FromSeconds(delta * Timescale);
        }
    }
}
