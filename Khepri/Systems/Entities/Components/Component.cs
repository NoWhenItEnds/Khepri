using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Utilities.ECS;

namespace Khepri.Entities.Components
{
    /// <summary> A single aspect of an entity. An entity's capabilities are defined by the components that construct it. </summary>
    [GlobalClass]
    public abstract partial class Component : Resource, IComponent
    {
        /// <summary>
        /// The entity this component is attached to. <c>null</c> before <see cref="Initialise"/> is called and again after
        /// <see cref="Detach"/> completes. Not exported — neither authored nor serialised.
        /// </summary>
        public Entity? Owner { get; private set; }


        /// <summary>
        /// Sets <see cref="Owner"/> to <paramref name="owner"/>. Safe to call when <see cref="Owner"/> is already
        /// <paramref name="owner"/> (no-op). Throws if <see cref="Owner"/> is already bound to a <em>different</em>
        /// entity, because a component instance belongs to exactly one entity.
        /// </summary>
        /// <param name="owner"> The entity to bind this component to. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this component is already bound to a different entity — rebinding across entities is not permitted.
        /// </exception>
        internal void Initialise(Entity owner)
        {
            if (Owner is not null && !ReferenceEquals(Owner, owner))
            {
                throw new InvalidOperationException(
                    $"Component '{GetType().Name}' is already bound to a different entity and cannot be rebound.");
            }

            Boolean newlyBound = Owner is null;
            Owner = owner;
            if (newlyBound)
            {
                OnAttached();
            }
        }


        /// <summary> Clears <see cref="Owner"/>, leaving it <c>null</c>. Called by <see cref="Entity"/> on a successful remove. </summary>
        internal void Detach()
        {
            if (Owner is not null)
            {
                OnDetached();
            }
            Owner = null;
        }


        /// <summary> Called once when this component is bound to an entity, after <see cref="Owner"/> is set. The default is a no-op. </summary>
        /// <remarks> Override to react to attachment — for example subscribing to the owner's events or caching a sibling component. Runs before the entity raises its <c>ComponentAdded</c> event. </remarks>
        protected virtual void OnAttached()
        {
        }


        /// <summary> Called once when this component is removed from its entity, while <see cref="Owner"/> is still set. The default is a no-op. </summary>
        /// <remarks> Override to undo whatever <see cref="OnAttached"/> established — <see cref="Owner"/> remains available here so subscriptions can be released, and is cleared immediately afterwards. Runs before the entity raises its <c>ComponentRemoved</c> event. </remarks>
        protected virtual void OnDetached()
        {
        }


        /// <summary> Resolves any spawn-time state once the component has been duplicated onto a fresh entity — for example rolling a range to a concrete value, or instantiating authored container contents. </summary>
        /// <remarks> The default is a no-op; components whose authored values are already their runtime values need not override it. Not called on the restore path, so rolled values stay fixed across a save/load. </remarks>
        /// <param name="ancestry"> The <see cref="EntityPrefab"/>s currently open on the instantiation stack; container components forward it to <see cref="EntityPrefab.Instantiate(ISet{EntityPrefab})"/> to reject a prefab that transitively contains itself. </param>
        public virtual void OnInstantiate(ISet<EntityPrefab> ancestry)
        {
        }


        /// <summary> Verifies this component's authored data once at load, before any entity is built from its prefab. The default is a no-op, for components with no authoring invariants to enforce. </summary>
        /// <remarks> Distinct from <see cref="OnInstantiate"/>: this asserts fixed facts about the authored template (a required reference was filled), whereas <see cref="OnInstantiate"/> resolves per-spawn state. Running it once at load surfaces authoring mistakes at boot rather than lazily when an entity of that prefab first spawns. </remarks>
        /// <param name="prefab"> The prefab this template belongs to, named in any error raised. </param>
        public virtual void Validate(EntityPrefab prefab)
        {
        }
    }
}
