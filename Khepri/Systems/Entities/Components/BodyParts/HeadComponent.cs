using Khepri.Entities.Prefabs;
using Khepri.Prefabs;

namespace Khepri.Entities.Components
{
    /// <summary> A component indicating that an entity possesses one or more heads. </summary>
    public class HeadComponent : BodyPartComponent
    {
        /// <summary> Initialises a new instance of the <see cref="HeadComponent"/> class. </summary>
        /// <param name="entity"> The entity that this body part is attached to. </param>
        public HeadComponent(Entity entity) : base(entity) { }


        /// <summary> Creates a <see cref="HeadComponent"/> from prefab data. </summary>
        /// <param name="entity"> The entity the component will be attached to. </param>
        /// <param name="data"> The component's parsed JSON data; no properties are consumed by this component. </param>
        /// <returns> A fully constructed <see cref="HeadComponent"/>. </returns>
        [ComponentFactory]
        private static HeadComponent Create(Entity entity, PrefabData data)
        {
            return new HeadComponent(entity);
        }
    }
}
