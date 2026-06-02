namespace Khepri.Entities.Components
{
    /// <summary> A component indicating that an entity possess one or more head(s). </summary>
    public class HeadComponent : BodyPartComponent
    {
        /// <summary> Initialises a new instance of the <see cref="HeadComponent"/> class. </summary>
        /// <param name="entity"> The entity that this body part is attached to. </param>
        public HeadComponent(Entity entity) : base(entity) { }
    }
}
