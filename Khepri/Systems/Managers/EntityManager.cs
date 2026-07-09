using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Data;
using Khepri.Data.Entities;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all the entities within the game world. Allows for simulating the entities separately from their rendered nodes. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> The solar system the player is currently within, whose entities are simulated every physics frame and handed to the renderer to draw. </summary>
        private SolarSystem _solarSystem = new SolarSystem();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<EntityManager>();


        /// <summary> Begin simulating an entity as part of the game world. </summary>
        /// <param name="entity"> The entity to add to the current solar system. </param>
        public void AddEntity(Entity entity)
        {
            _solarSystem.Add(entity);
        }


        /// <summary> Get all the entities that are currently tracked within the simulated game world. </summary>
        public IEnumerable<Entity> GetEntities() => _solarSystem.GetEntities();


        /// <inheritdoc/>
        public override void _Ready()
        {
            // Integrate only after every controller has applied its thrust and steering for the frame.
            // A high physics priority pushes this manager's step to the end of the physics tick, so no
            // control input is cleared before it has been consumed, whatever order the nodes sit in.
            ProcessPhysicsPriority = 100;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Drive gravity on the fixed physics clock so the integrator stays stable, scaling by the
            // world timescale so bodies move in step with the galaxy's universal time. Before the game
            // manager has readied the timescale reads as zero, leaving the system momentarily at rest.
            Single timescale = GameManager.Instance?.Timescale ?? 0f;
            _solarSystem.Step((Single)delta * timescale);
        }
    }
}
