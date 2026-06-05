using System;
using Godot;
using Khepri.Descriptions;

namespace Khepri.Entities.Components
{
    /// <summary> Declares the noun an entity is called by — the core of its name. </summary>
    /// <remarks> The most salient kind across an entity's components wins, so a transformation that swaps this component (a goblin becoming a statue) renames the entity without any separate bookkeeping. </remarks>
    [GlobalClass]
    public partial class KindComponent : Component
    {
        /// <summary> The noun this component claims for its entity, for example <c>"goblin"</c>. </summary>
        [Export] public String Noun { get; set; } = String.Empty;

        /// <summary> How strongly this noun defines the entity; the highest salience across the entity's components becomes its name. </summary>
        [Export] public Int32 Salience { get; set; } = 0;


        /// <inheritdoc/>
        public override void Contribute(NameBuilder builder)
        {
            builder.Noun(Noun, Salience);
        }
    }
}
