using System;
using Godot;

namespace Khepri.Entities.Components.Skills
{
    /// <summary> A single skill known by an entity — one entity's proficiency in a shared <see cref="SkillKind"/>. </summary>
    [GlobalClass]
    public partial class EntitySkill : Resource
    {
        /// <summary> The highest level any skill can reach; whole numbers map to the dots shown in the UI. </summary>
        public const Single MaxLevel = 5f;

        /// <summary> The shared definition of which skill this is. </summary>
        [Export] public SkillKind Kind { get; set; } = null!;

        /// <summary> The entity's theoretical knowledge — what it thinks it knows, gained from lectures and books. Held continuously over <c>0..</c><see cref="MaxLevel"/>. </summary>
        [Export] public Single TheoreticalLevel { get; set; } = 0f;

        /// <summary> The entity's practical knowledge — what it can actually do, earned through practice. Held continuously over <c>0..</c><see cref="MaxLevel"/>. </summary>
        [Export] public Single PracticalLevel { get; set; } = 0f;


        /// <summary> The whole-dot rating (0–5) of <see cref="TheoreticalLevel"/>, for display. </summary>
        public Int32 TheoreticalDots => (Int32)Math.Floor(TheoreticalLevel);

        /// <summary> The whole-dot rating (0–5) of <see cref="PracticalLevel"/>, for display. </summary>
        public Int32 PracticalDots => (Int32)Math.Floor(PracticalLevel);


        /// <summary> Advances the entity's theoretical knowledge, clamped to <c>0..</c><see cref="MaxLevel"/>. </summary>
        /// <param name="amount"> The amount to add; may be negative to represent forgetting. </param>
        public void Study(Single amount)
        {
            TheoreticalLevel = Math.Clamp(TheoreticalLevel + amount, 0f, MaxLevel);
        }


        /// <summary> Advances the entity's practical knowledge, clamped to <c>0..</c><see cref="MaxLevel"/>. </summary>
        /// <param name="amount"> The amount to add; may be negative. </param>
        public void Practise(Single amount)
        {
            PracticalLevel = Math.Clamp(PracticalLevel + amount, 0f, MaxLevel);
        }
    }
}
