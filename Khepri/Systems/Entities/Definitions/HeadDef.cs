using System.Collections.Generic;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities.Definitions
{
    /// <summary> Authored definition of a <see cref="HeadComponent"/>; carries no parameters. </summary>
    [GlobalClass]
    public partial class HeadDef : ComponentDef
    {
        /// <inheritdoc/>
        public override Component Create(Entity owner, ISet<EntityPrefab> ancestry)
        {
            return new HeadComponent(owner);
        }
    }
}
