using Khepri.Entities;
using Khepri.Rooms;

namespace Khepri.Controllers
{
    /// <summary> The "brain" of an entity: the decision-maker that performs the entity's action when its turn comes. </summary>
    /// <remarks>
    /// One controller drives one entity (its <see cref="Owner"/>). The reference is deliberately one-way — a controller knows its entity, but an <see cref="Entity"/> never references its controller — so the POCO entity stays controller-agnostic and a Godot-aware subclass (such as <c>PlayerController</c>) cannot leak engine types into the domain.
    /// The base is Godot-free; only subclasses that need it (input, etc.) take a Godot dependency.
    /// </remarks>
    public abstract class EntityController
    {
        /// <summary> The entity this controller drives. </summary>
        public Entity Owner { get; }


        /// <summary> Initialises a new controller bound to the entity it will drive. </summary>
        /// <param name="owner"> The entity this brain controls. </param>
        protected EntityController(Entity owner)
        {
            Owner = owner;
        }


        /// <summary> Performs this entity's action for the current turn within <paramref name="room"/>, the room its owner currently occupies. </summary>
        /// <remarks> The room is the controller's whole view of the world: <see cref="Room.GetEntities"/> gives who is present and <see cref="Room.GetConnections"/> the exits. When controllers gain the ability to <em>change</em> the world (move, attack), those verbs will be injected here rather than reached for via a manager singleton. </remarks>
        /// <param name="room"> The room the owner is in. </param>
        public abstract void Act(Room room);
    }
}
