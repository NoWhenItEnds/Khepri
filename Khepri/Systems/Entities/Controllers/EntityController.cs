using System;
using System.Collections.Generic;
using Jaypen.Utilities.ECS;
using Khepri.Entities.Abilities;
using Khepri.Entities.Actions;
using Khepri.Rooms;

namespace Khepri.Entities.Controllers
{
    /// <summary> The "brain" of an entity: the decision-maker that performs the entity's action when its turn comes. </summary>
    /// <remarks> One controller drives one entity (its <see cref="Entity"/>). The reference is deliberately one-way — a controller knows its entity, but an <see cref="Entities.Entity"/> never references its controller. </remarks>
    public abstract class EntityController : ISingleComponentHolder<EntityAbility>
    {
        /// <summary> The entity this controller drives. </summary>
        public Entity Entity { get; }


        /// <summary> Delegate storage for all abilities attached to this controller — at most one per exact concrete type. </summary>
        private readonly ComponentStore<EntityAbility> _abilities = new ComponentStore<EntityAbility>();

        /// <summary> The action the controller has chosen but not yet taken, or <c>null</c> while awaiting input. </summary>
        private EntityAction? _pendingAction;


        /// <summary> Initialises a new controller bound to the entity it will drive. </summary>
        /// <param name="entity"> The entity this brain controls. </param>
        protected EntityController(Entity entity)
        {
            Entity = entity;
        }


        /// <summary> Raised after an ability is successfully attached to this controller, passing the newly added ability as the argument. </summary>
        /// <remarks> Subscribers are invoked synchronously after the internal collection has already been mutated — the controller's ability set already reflects the change when handlers run. </remarks>
        public event Action<EntityAbility>? ComponentAdded;


        /// <summary> Raised after an ability is successfully detached from this controller, passing the removed ability as the argument. </summary>
        /// <remarks> Subscribers are invoked synchronously after the internal collection has already been mutated — the controller's ability set already reflects the change when handlers run. </remarks>
        public event Action<EntityAbility>? ComponentRemoved;


        /// <summary> Adds an ability instance to this controller. </summary>
        /// <param name="component"> The ability to attach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the ability was added; <c>false</c> if an ability of the same concrete type is already attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="EntityAbility.Initialise"/> (which sets <c>Owner</c>) before raising
        /// <see cref="ComponentAdded"/>, so observers always see a fully initialised ability.
        /// </remarks>
        public Boolean AddComponent(EntityAbility component)
        {
            Boolean added = _abilities.Add(component);
            if (added) { component.Initialise(this); ComponentAdded?.Invoke(component); }
            return added;
        }


        /// <summary> Returns all abilities currently attached to this controller, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the controller's internal ability set. </remarks>
        /// <returns> A read-only snapshot of every attached ability. </returns>
        public IReadOnlyCollection<EntityAbility> GetComponents() => _abilities.GetAll();


        /// <summary> Checks whether an ability whose concrete runtime type is exactly <typeparamref name="TComponent"/> is currently attached. </summary>
        /// <typeparam name="TComponent"> The exact concrete ability type to test for. </typeparam>
        /// <returns> <c>true</c> if an ability of that exact type is attached; <c>false</c> otherwise. </returns>
        public Boolean HasComponent<TComponent>() where TComponent : EntityAbility => _abilities.Has<TComponent>();


        /// <summary> Attempts to retrieve the attached ability whose concrete runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <remarks>
        /// Uses an exact-type dictionary lookup, not assignability scanning. A subclass of <typeparamref name="TComponent"/> stored under its
        /// own key will not be found here.
        /// Because <typeparamref name="TComponent"/> is constrained to a reference type in this implementation, <c>default</c> on a miss is
        /// <c>null</c>; callers should still use the Boolean return value as the authoritative presence check.
        /// </remarks>
        /// <typeparam name="TComponent"> The exact concrete ability type to retrieve. </typeparam>
        /// <param name="component"> Contains the attached ability when this method returns <c>true</c>; otherwise <c>default(<typeparamref name="TComponent"/>)</c>. </param>
        /// <returns> <c>true</c> if the matching ability was found; <c>false</c> if none is attached. </returns>
        public Boolean TryGetComponent<TComponent>(out TComponent component) where TComponent : EntityAbility
        {
            return _abilities.TryGet<TComponent>(out component);
        }


        /// <summary> Removes the attached ability whose runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The type of ability to remove. </typeparam>
        /// <returns> <c>true</c> if an ability was removed; <c>false</c> if none was found. </returns>
        /// <remarks>
        /// On success: calls <see cref="EntityAbility.Detach"/> (which clears <c>Owner</c>) before raising
        /// <see cref="ComponentRemoved"/>, so <c>Owner</c> is already <c>null</c> when observers see the removal.
        /// </remarks>
        public Boolean RemoveComponent<TComponent>() where TComponent : EntityAbility
        {
            Boolean removed = _abilities.Remove<TComponent>(out EntityAbility? removedComponent);
            if (removed) { removedComponent!.Detach(); ComponentRemoved?.Invoke(removedComponent); }
            return removed;
        }


        /// <summary> Removes a specific ability instance from this controller. </summary>
        /// <param name="component"> The exact ability instance to detach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the ability was removed; <c>false</c> if it was not attached or a different instance of the same type is attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="EntityAbility.Detach"/> (which clears <c>Owner</c>) before raising
        /// <see cref="ComponentRemoved"/>, so <c>Owner</c> is already <c>null</c> when observers see the removal.
        /// </remarks>
        public Boolean RemoveComponent(EntityAbility component)
        {
            Boolean removed = _abilities.Remove(component);
            if (removed) { component.Detach(); ComponentRemoved?.Invoke(component); }
            return removed;
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
