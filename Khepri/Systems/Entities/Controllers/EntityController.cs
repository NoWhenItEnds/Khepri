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


        /// <summary> Whether this controller is ready to take its turn right now. </summary>
        /// <remarks> Synchronous brains (such as AI) are always ready; an input-driven brain like <see cref="PlayerController"/> is ready only once the player has chosen an action, which lets the <see cref="Managers.TurnManager"/> pause the turn order on the player's turn until input arrives. </remarks>
        public virtual Boolean IsReady => true;


        /// <summary> Initialises a new controller bound to the entity it will drive. </summary>
        /// <param name="entity"> The entity this brain controls. </param>
        protected EntityController(Entity entity)
        {
            Entity = entity;
        }


        /// <summary> Decides this entity's action for the current turn within <paramref name="room"/>, the room its owner currently occupies. </summary>
        /// <remarks> The room is the controller's whole view of the world: <see cref="Room.GetEntities"/> gives who is present and <see cref="Room.GetConnections"/> the exits. The brain only <em>decides</em> — it constructs and returns the action to perform but never mutates the world itself; the returned <see cref="EntityAction"/> self-executes when the scheduler performs it. Returns <c>null</c> when the brain has no action this turn (for example, a player still awaiting input). </remarks>
        /// <param name="room"> The room the owner is in. </param>
        /// <returns> The action to perform, or <c>null</c> to take no action this turn. </returns>
        public abstract EntityAction? Act(Room room);
    }
}
