using System;

namespace Khepri.Entities.Components
{
    /// <summary> A component giving an entity the ability to be damaged. </summary>
    public class HealthComponent : Component
    {
        /// <summary> The maximum amount of health this entity can have. </summary>
        public Int32 Max { get; }


        /// <summary> Initialises a new <see cref="HealthComponent"/> with a concrete maximum health value. </summary>
        /// <param name="entity"> The parent entity. </param>
        /// <param name="max"> The concrete maximum health value. </param>
        public HealthComponent(Entity entity, Int32 max) : base(entity)
        {
            Max = max;
            // TODO - Extend to have current health, defence, etc.
        }
    }
}
