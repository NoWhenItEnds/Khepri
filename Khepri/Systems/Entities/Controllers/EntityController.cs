using System;
using Khepri.Entities.Actions;
using Khepri.Rooms;

namespace Khepri.Entities.Controllers
{
    /// <summary> The "brain" of an entity: the decision-maker that performs the entity's action when its turn comes. </summary>
    /// <remarks> One controller drives one entity (its <see cref="Entity"/>). The reference is deliberately one-way — a controller knows its entity, but an <see cref="Entities.Entity"/> never references its controller. </remarks>
    public abstract class EntityController
    {
        /// <summary> The entity this controller drives. </summary>
        public Entity Entity { get; }


        /// <summary> The action the controller has chosen but not yet taken, or <c>null</c> while awaiting input. </summary>
        private EntityAction? _pendingAction;


        /// <summary> Initialises a new controller bound to the entity it will drive. </summary>
        /// <param name="entity"> The entity this brain controls. </param>
        protected EntityController(Entity entity)
        {
            Entity = entity;
        }


        /// <summary> Attempts to queue the given action for execution on the entity's next turn. </summary>
        /// <param name="action"> The action to queue. </param>
        /// <returns> <c>true</c> if the action was queued; <c>false</c> if the controller is already waiting on another action. </returns>
        public Boolean TryQueueAction(EntityAction action)
        {
            Boolean canQueue = _pendingAction == null;
            if(canQueue)
            {
                _pendingAction = action;
            }
            return canQueue;
        }


        /// <summary> Decides this entity's action for the current turn within <paramref name="room"/>, the room its owner currently occupies. </summary>
        /// <param name="room"> The room the owner is in. The room is the controller's whole view of the world: <see cref="Room.GetEntities"/> gives who is present and <see cref="Room.GetConnections"/> the exits. </param>
        /// <returns> The outcome of the action, or <see cref="ActionResult.None"/> if no action was taken. </returns>
        public ActionResult Act(Room room)
        {
            EntityAction? action = _pendingAction;
            _pendingAction = null;
            return action?.Perform(room) ?? ActionResult.None;
        }
    }
}
