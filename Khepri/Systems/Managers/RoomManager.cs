using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing the game's rooms and connections between them. </summary>
    public partial class RoomManager : SingletonNode<RoomManager>
    {
        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<RoomManager>();


        // TODO - This game world should be defined by prefabs, not randomly generated, but also support additional user creations via connections.
    }
}
