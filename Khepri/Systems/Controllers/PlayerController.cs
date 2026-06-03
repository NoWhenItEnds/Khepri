using Jaypen.Utilities.Logging;
using Khepri.Entities;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;

namespace Khepri.Controllers
{
    /// <summary> The brain for the human-controlled entity. Its turn is driven by player input rather than by the scheduler asking it what to do. </summary>
    /// <remarks> <c>TurnManager</c> invokes <see cref="Act"/> when the player commits an action; the world is then advanced for every other actor. For now the action is a simple "wait", a placeholder for input-derived intent (movement, interaction). </remarks>
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
            Logger.LogInformation("Player ({Uid}) takes a turn in room {Room}.", Owner.UId, room.UId);
        }
    }
}
