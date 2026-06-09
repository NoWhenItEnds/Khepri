using System;
using Khepri.Managers;
using Khepri.Rooms;

namespace Khepri.Entities.Actions
{
    /// <summary> Moves the acting entity into a directly-connected room. </summary>
    public sealed class MoveAction : EntityAction
    {
        /// <summary> The room the actor is attempting to move into. </summary>
        private readonly Room _destination;


        /// <summary> Initialises a new move into <paramref name="destination"/>. </summary>
        /// <param name="actor"> The entity to move. </param>
        /// <param name="destination"> The room to move into; must be directly connected to the actor's current room. </param>
        public MoveAction(Entity actor, Room destination) : base(actor)
        {
            _destination = destination;
        }


        /// <inheritdoc/>
        public override ActionResult Perform(Room room)
        {
            Boolean moved = RoomManager.Instance!.MoveEntity(Actor, _destination);

            return moved ? ActionResult.Succeeded : ActionResult.Failed;
        }
    }
}
