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


        /// <summary> The action the player has chosen but not yet taken, or <c>null</c> while awaiting input. </summary>
        private EntityAction? _pendingAction;


        /// <summary> Initialises a new player controller for the given entity. </summary>
        /// <param name="owner"> The player's entity. </param>
        public PlayerController(Entity owner) : base(owner)
        {
        }


        /// <inheritdoc/>
        public override Boolean IsReady => _pendingAction is not null;


        /// <summary> Queues the room the player chose as the move they will make on their next turn. </summary>
        /// <remarks> Called from the UI when the player selects a room; the controller translates that raw choice into a concrete <see cref="MoveAction"/>, which <see cref="Act"/> hands to the <see cref="Managers.TurnManager"/>. A later selection replaces an unspent earlier one. </remarks>
        /// <param name="destination"> The room the player chose to move into. </param>
        public void Select(Room destination)
        {
            _pendingAction = new MoveAction(Entity, destination);
        }


        /// <inheritdoc/>
        public override EntityAction? Act(Room room)
        {
            EntityAction? action = _pendingAction;
            _pendingAction = null;

            Logger.LogInformation("Player ({Uid}) acts in room {Room}.", Entity.UId, room.UId);

            return action;
        }
    }
}
