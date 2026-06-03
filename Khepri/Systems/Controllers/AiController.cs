using Jaypen.Utilities.Logging;
using Khepri.Entities;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;

namespace Khepri.Controllers
{
    /// <summary> A placeholder non-player brain. The scheduler asks it to act once per world step. </summary>
    /// <remarks> A stub for now: it observes its surroundings and logs. Real behaviour (wander, pursue, attack) will be driven from the same <see cref="Act"/> entry point as the verb set and <see cref="TurnContext"/> grow. </remarks>
    public sealed class AiController : EntityController
    {
        /// <summary> The logger instance the controller uses. </summary>
        private static readonly ILogger Logger = Log.For<AiController>();


        /// <summary> Initialises a new AI controller for the given entity. </summary>
        /// <param name="owner"> The entity this brain drives. </param>
        public AiController(Entity owner) : base(owner)
        {
        }


        /// <inheritdoc/>
        public override void Act(Room room)
        {
            int others = room.GetEntities().Count - 1;   // GetEntities includes this actor.
            Logger.LogInformation("Entity ({Uid}) surveys room {Room}; sees {Others} other(s).", Owner.UId, room.UId, others);
        }
    }
}
