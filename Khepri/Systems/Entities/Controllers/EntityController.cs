using Khepri.Rooms;

namespace Khepri.Entities.Controllers
{
    /// <summary> The "brain" of an entity: the decision-maker that performs the entity's action when its turn comes. </summary>
    /// <remarks> One controller drives one entity (its <see cref="Entity"/>). The reference is deliberately one-way — a controller knows its entity, but an <see cref="Entities.Entity"/> never references its controller. </remarks>
    public abstract class EntityController
    {
        /// <summary> The entity this controller drives. </summary>
        public Entity Entity { get; }


        /// <summary> Initialises a new controller bound to the entity it will drive. </summary>
        /// <param name="entity"> The entity this brain controls. </param>
        protected EntityController(Entity entity)
        {
            Entity = entity;
        }


        /// <summary> Performs this entity's action for the current turn within <paramref name="room"/>, the room its owner currently occupies. </summary>
        /// <remarks> The room is the controller's whole view of the world: <see cref="Room.GetEntities"/> gives who is present and <see cref="Room.GetConnections"/> the exits. When controllers gain the ability to <em>change</em> the world (move, attack), those verbs will be injected here rather than reached for via a manager singleton. </remarks>
        /// <param name="room"> The room the owner is in. </param>
        public abstract void Act(Room room);
    }
}
