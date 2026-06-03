using System;
using Khepri.Entities.Prefabs;
using Khepri.Prefabs;

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


        /// <summary> Creates a <see cref="SkillComponent"/> from prefab data. </summary>
        /// <param name="entity"> The entity the component will be attached to. </param>
        /// <param name="data"> The component's parsed JSON data; no properties are consumed by this component. </param>
        /// <returns> A fully constructed <see cref="SkillComponent"/>. </returns>
        [ComponentFactory]
        private static SkillComponent Create(Entity entity, PrefabData data)
        {
            return new SkillComponent(entity);
        }
    }


    /// <summary> A single skill known by an entity. </summary>
    /// <remarks> Skills with the same type / name are the same skill, just different instances for different entities. </remarks>
    public record EntitySkill   // TODO - Add kind enums / look up dictionary for names? "Computer : Academic, Melee : Combat"
    {
        /// <summary> The name / type of the skill. </summary>
        public required String Type { get; init; }

        /// <summary> The entity's theoretical knowledge of the skill. What they think they know. </summary>
        public Single TheoreticalLevel { get; private set; } = 0f;

        /// <summary> The entity's actual knowledge of the skill. What they can actually do. </summary>
        public Single PracticalLevel { get; private set; } = 0f;
    }
}
