using System;
using Godot;
using Jaypen.Utilities.ECS;
using Khepri.Entities.Actions;
using Khepri.Entities.Controllers;

namespace Khepri.Entities.Abilities
{
    /// <summary> A single ability that an entity, via a controller, can perform. All actions are facilitated by an ability, and the logic therein. </summary>
    [GlobalClass]
    public abstract partial class EntityAbility : Resource, IComponent
    {
        /// <summary> The initial number of seconds the ability takes to execute. </summary>
        [Export] private Single _baseCost;


        /// <summary>
        /// The controller this ability is attached to. <c>null</c> before <see cref="Initialise"/> is called and again after
        /// <see cref="Detach"/> completes. Not exported — neither authored nor serialised.
        /// </summary>
        public EntityController? Owner { get; private set; }


        /// <summary>
        /// Sets <see cref="Owner"/> to <paramref name="owner"/>. Safe to call when <see cref="Owner"/> is already
        /// <paramref name="owner"/> (no-op). Throws if <see cref="Owner"/> is already bound to a <em>different</em>
        /// controller, because an ability instance belongs to exactly one controller.
        /// </summary>
        /// <param name="owner"> The controller to bind this ability to. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this ability is already bound to a different controller — rebinding across controllers is not permitted.
        /// </exception>
        internal void Initialise(EntityController owner)
        {
            if (Owner is not null && !ReferenceEquals(Owner, owner))
            {
                throw new InvalidOperationException(
                    $"Ability '{GetType().Name}' is already bound to a different controller and cannot be rebound.");
            }

            Owner = owner;
        }


        /// <summary> Clears <see cref="Owner"/>, leaving it <c>null</c>. Called by <see cref="EntityController"/> on a successful remove. </summary>
        internal void Detach()
        {
            Owner = null;
        }


        /// <summary> Perform the ability. </summary>
        /// <param name="action"> The queued action that contains the stateful information needed to perform the ability. </param>
        /// <returns> Whether the ability was successfully completed. </returns>
        public abstract ActionResult Perform(EntityAction action);  // TODO - Do we need actions for everything. It's just a data class. What about variants?
    }
}
