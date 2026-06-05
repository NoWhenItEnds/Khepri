using System;
using Godot;
using Khepri.Descriptions;

namespace Khepri.Entities.Components
{
    /// <summary> Declares the material an entity is made of, contributing it as an adjective to the entity's name. </summary>
    [GlobalClass]
    public partial class MaterialComponent : Component
    {
        /// <summary> The material adjective, for example <c>"iron"</c>. </summary>
        [Export] public String Material { get; set; } = String.Empty;

        /// <summary> Orders the material adjective among others; material words sit close to the noun, so a high rank. </summary>
        [Export] public Int32 Rank { get; set; } = 60;


        /// <inheritdoc/>
        public override void Contribute(NameBuilder builder)
        {
            builder.Adjective(Material, Rank);
        }
    }
}
