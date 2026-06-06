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

        /// <summary> The broad discipline this skill belongs to, used to group skills in the UI and, later, to gate spells against a whole category. </summary>
        [Export] public SkillCategory Category { get; set; } = SkillCategory.Academic;

        /// <summary> A short description of what the skill covers, shown to the player. </summary>
        [Export(PropertyHint.MultilineText)] public String Summary { get; set; } = String.Empty;
    }


    /// <summary> The broad disciplines skills are grouped under — adapted from Vampire the Masquerade's Physical / Social / Mental columns for a school of magic. </summary>
    public enum SkillCategory
    {
        /// <summary> Book knowledge and theory — arithmancy, history of magic, herbology. </summary>
        Academic,

        /// <summary> The practising magical disciplines spells are cast through — evocation, divination, wardcraft. </summary>
        Arcane,

        /// <summary> Dealing with people — persuasion, etiquette, intimidation. </summary>
        Social,

        /// <summary> Bodily prowess — athletics, stealth, broomwork. </summary>
        Physical
    }
}
