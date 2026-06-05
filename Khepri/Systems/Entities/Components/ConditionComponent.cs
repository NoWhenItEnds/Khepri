using System;
using Godot;
using Khepri.Descriptions;

namespace Khepri.Entities.Components
{
    /// <summary> Declares the visible condition of an entity, contributing it as an adjective to the entity's name. </summary>
    [GlobalClass]
    public partial class ConditionComponent : Component
    {
        /// <summary> The condition adjective, for example <c>"battered"</c>. </summary>
        [Export] public String Condition { get; set; } = String.Empty;

        /// <summary> Orders the condition adjective among others; condition and opinion words sit furthest from the noun, so a low rank. </summary>
        [Export] public Int32 Rank { get; set; } = 20;


        /// <inheritdoc/>
        public override void Contribute(NameBuilder builder)
        {
            builder.Adjective(Condition, Rank);
        }
    }
}
