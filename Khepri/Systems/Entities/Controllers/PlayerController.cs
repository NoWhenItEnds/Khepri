using Jaypen.Utilities.Logging;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;

namespace Khepri.Entities.Controllers
{
    /// <summary> The brain for the human-controlled entity. Its turn is driven by player input rather than by the scheduler asking it what to do. </summary>
    public sealed class PlayerController : EntityController
    {
        /// <summary> The logger instance the controller uses. </summary>
        private static readonly ILogger Logger = Log.For<PlayerController>();


        /// <summary> Initialises a new player controller for the given entity. </summary>
        /// <param name="owner"> The player's entity. </param>
        public PlayerController(Entity owner) : base(owner)
        {
        }


        /// <inheritdoc/>
        public override void Act(Room room)
        {
            // TODO - Translate the player's input (move, interact, etc.) into a concrete action against the room.
            Logger.LogInformation("Player ({Uid}) takes a turn in room {Room}.", Entity.UId, room.UId);
        }
    }
}
