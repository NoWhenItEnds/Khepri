namespace Khepri.Entities.Components
{
    /// <summary> A kind of body part that an entity possesses. </summary>
    public abstract class BodyPartComponent : Component
    {
        /// <summary> Initialises a new instance of the <see cref="BodyPartComponent"/> class. </summary>
        /// <param name="entity"> The entity that this body part is attached to. </param>
        public BodyPartComponent(Entity entity) : base(entity) { }

        // TODO - Add equipment slots. These will give access to wearing clothes, with have inventory components.

        // TODO - Some body parts also have inherent inventory slots.
    }
}
