using System;
using Godot;

namespace Khepri.Entities.Components.Skills
{
    /// <summary> An authored definition of a single skill an entity can learn — shared by every entity that knows it. </summary>
    [GlobalClass]
    public partial class SkillKind : Resource
    {
        /// <summary> The skill's name, for example <c>"Evocation"</c> or <c>"Herbology"</c>. </summary>
        [Export] public String Noun { get; set; } = String.Empty;

        /// <summary> The broad discipline this skill belongs to. </summary>
        [Export] public SkillCategory Category { get; set; } = SkillCategory.Academic;

        /// <summary> A short description of what the skill covers, shown to the player. </summary>
        [Export(PropertyHint.MultilineText)] public String Summary { get; set; } = String.Empty;
    }


    /// <summary> The broad disciplines skills are grouped under.. </summary>
    public enum SkillCategory
    {
        Academic,
        Arcane,
        Social,
        Physical
    }
}
