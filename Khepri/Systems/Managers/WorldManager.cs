using Jaypen.Logging;
using Jaypen.Singletons;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The central manager for all the nodes within the player's current view. Decoupled from the entity data objects. </summary>
    public partial class WorldManager : SingletonNode2D<WorldManager>
    {
        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<WorldManager>();
    }
}
