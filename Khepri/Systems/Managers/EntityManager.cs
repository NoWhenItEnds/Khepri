using Jaypen.Logging;
using Jaypen.Singletons;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all the entities within the game world. Allows for simulating the entities separately from their rendered nodes. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<EntityManager>();
    }
}
