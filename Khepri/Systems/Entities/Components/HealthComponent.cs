using System;

namespace Khepri.Entities.Components
{
    public class HealthComponent : Component
    {
        /// <summary> A component giving an entity the ability to be damaged. </summary>
        /// <param name="entity"> The parent entity. </param>
        /// <param name="initialMax"> The initial maximum amount of health. </param>
        public HealthComponent(Entity entity, Int32 initialMax) : base(entity)
        {
            // TODO - Extend to have defence, etc.
        }
    }
}
