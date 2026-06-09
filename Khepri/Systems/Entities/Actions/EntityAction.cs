using Khepri.Rooms;

namespace Khepri.Entities.Actions
{
    /// <summary> A single discrete thing an entity attempts on its turn: the unit a brain produces and the scheduler performs. </summary>
    public abstract class EntityAction
    {
        /// <summary> The entity performing the action. </summary>
        public Entity Actor { get; }


        /// <summary> Initialises a new action bound to the entity that will perform it. </summary>
        /// <param name="actor"> The entity performing the action. </param>
        protected EntityAction(Entity actor)
        {
            Actor = actor;
        }


        /// <summary> Carries out the action against the world. </summary>
        /// <param name="room"> The room the action is being performed in. </param>
        /// <returns> Whether the action took effect. </returns>
        public abstract ActionResult Perform(Room room);
    }


    /// <summary> The outcome of attempting an <see cref="EntityAction"/>. </summary>
    public enum ActionResult
    {
        /// <summary> No action was attempted (the brain produced nothing this turn). </summary>
        None = 0,

        /// <summary> The action took effect. </summary>
        Succeeded = 1,

        /// <summary> The action was attempted but rejected by the world (e.g. an illegal move). </summary>
        Failed = 2,
    }
}
