using System.Collections.Generic;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Data.Entities;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all the entities within the game world. Allows for simulating the entities separately from their rendered nodes. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> All the entities that are currently tracked within the simulated game world. </summary>
        private HashSet<Entity> _entities = new HashSet<Entity>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<EntityManager>();


        /// <summary> Get an array of all the entities that are currently tracked within the simulated game world. </summary>
        public IEnumerable<Entity> GetEntities() => _entities;
    }
}
