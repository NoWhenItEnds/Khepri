using System;

namespace Khepri.Entities.Components
{
    /// <summary> A component allowing an entity to learn, progress, and use skills. </summary>
    public class SkillComponent : Component
    {
        /// <summary> Initialises a new instance of the <see cref="SkillComponent"/> class. </summary>
        /// <param name="entity"> The entity that has this skill. </param>
        public SkillComponent(Entity entity) : base(entity)
        {
            // TODO - Create dictionary of skills and values.
        }
    }
}
