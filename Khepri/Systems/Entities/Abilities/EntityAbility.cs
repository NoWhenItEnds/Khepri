using System;
using Godot;
using Khepri.Entities.Actions;

namespace Khepri.Entities.Abilities
{
    /// <summary> A single ability that an entity, via a controller, can perform. All actions are facilitated by an ability, and the logic therein. </summary>
    [GlobalClass]
    public abstract partial class EntityAbility : Resource
    {
        /// <summary> The initial number of seconds the ability takes to execute. </summary>
        [Export] private Single _baseCost;


        /// <summary> Perform the ability. </summary>
        /// <param name="action"> The queued action that contains the stateful information needed to perform the ability. </param>
        /// <returns> Whether the ability was successfully completed. </returns>
        public abstract ActionResult Perform(EntityAction action);  // TODO - Do we need actions for everything. It's just a data class. What about variants?
    }
}
