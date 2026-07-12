using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.World;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for the galaxy's solar systems: owns them and drives their simulation on the physics clock, separately from the rendered nodes. </summary>
    public partial class SolarSystemManager : SingletonNode<SolarSystemManager>
    {
        /// <summary> The game world's current timescale: how many simulated seconds pass for each real second. </summary>
        [ExportGroup("Settings")]
        [Export] public Single Timescale { get; private set; } = 12f;

        /// <summary> The longest simulated timestep, in seconds, a single integrator pass may cover; systems split longer steps into substeps. Smaller is more accurate but costs more passes at high timescale; size it against the fastest orbit the game authors. </summary>
        [Export(PropertyHint.Range, "0.01,1,0.01,or_greater")] private Single _maxStepDuration = 0.2f;


        /// <summary> Every solar system currently being simulated, stepped in creation order so the simulation is deterministic. </summary>
        private readonly List<SolarSystem> _solarSystems = new List<SolarSystem>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<SolarSystemManager>();


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Drive the simulation on the fixed physics clock so the integrator stays stable, scaling
            // by the timescale so every system's clock advances together.
            Single scaledDelta = (Single)delta * Timescale;

            foreach (SolarSystem system in _solarSystems)
            {
                system.Step(scaledDelta, _maxStepDuration);
            }
        }


        /// <summary> Adds a new solar system to the simulation. </summary>
        /// <returns> The newly added solar system. </returns>
        public SolarSystem AddSystem()
        {
            SolarSystem system = new SolarSystem();
            _solarSystems.Add(system);
            return system;
        }


        /// <summary> Get an immutable collection of all solar systems within the game world. </summary>
        public IEnumerable<SolarSystem> GetSolarSystems() => _solarSystems;


        /// <summary> Remove every system from the simulation. Used when a save is restored, so the world rebuilds from nothing; abandoned entities keep their stale system reference and fall out of view on the next cull. </summary>
        public void ClearSystems()
        {
            _solarSystems.Clear();
        }
    }
}
