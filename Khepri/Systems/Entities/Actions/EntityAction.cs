namespace Khepri.Entities.Actions
{
    /// <summary> A single discrete thing an entity attempts on its turn: the unit a brain produces and the scheduler performs. </summary>
    /// <remarks> An action is self-executing — it carries both what is intended and how to carry it out via <see cref="Perform"/>. The deciding brain (<see cref="Controllers.EntityController.Act"/>) only constructs the action; it never mutates the world itself. </remarks>
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
        /// <returns> Whether the action took effect. </returns>
        public abstract ActionResult Perform();
    }
}
