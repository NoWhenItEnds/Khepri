using System;
using System.Linq;
using Godot;

namespace Khepri.Entities.Components.Skills
{
    /// <summary> A component allowing an entity to learn, progress, and use skills. </summary>
    [GlobalClass]
    public partial class SkillComponent : Component
    {
        /// <summary> The skills this entity knows, authored in the Inspector. Each entry pairs a shared <see cref="SkillKind"/> with this entity's proficiency in it. </summary>
        [Export] public Godot.Collections.Array<EntitySkill> Skills { get; set; } = new Godot.Collections.Array<EntitySkill>();


        /// <summary> Finds this entity's proficiency in a given skill. </summary>
        /// <param name="kind"> The skill to look up. </param>
        /// <returns> The matching <see cref="EntitySkill"/>, or <c>null</c> if this entity has never learned that skill. </returns>
        public EntitySkill? GetSkill(SkillKind kind) => Skills.FirstOrDefault(skill => skill.Kind == kind);


        /// <inheritdoc/>
        public override void Validate(EntityPrefab prefab)
        {
            foreach (EntitySkill skill in Skills)
            {
                if (skill.Kind is null)
                {
                    throw new InvalidOperationException(
                        $"Entity prefab '{prefab.Name}' has a SkillComponent entry with no SkillKind assigned. Every authored skill must reference a kind.");
                }
            }
        }
    }
}
