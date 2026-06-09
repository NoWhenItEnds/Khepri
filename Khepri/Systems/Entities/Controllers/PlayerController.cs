using System;
using Jaypen.Utilities.Logging;
using Khepri.Entities.Actions;
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


        /// <summary> Moves the player's entity to the specified room. </summary>
        /// <param name="destination"> The room the player chose to move into. </param>
        public void MoveTo(Room destination)
        {
            MoveAction action = new MoveAction(Entity, destination);
            if (!TryQueueAction(action))
            {
                Logger.LogWarning("Failed to queue move action for player ({Uid}) to room {Room}.", Entity.UId, destination.UId);
            }
        }
    }
}
