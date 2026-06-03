using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities.Definitions
{
    /// <summary> Authored definition of a <see cref="SkillComponent"/>; carries no parameters. </summary>
    [GlobalClass]
    public partial class SkillDef : ComponentDef
    {
        /// <inheritdoc/>
        public override Component Create(Entity owner)
        {
            return new SkillComponent(owner);
        }
    }
}
