using System;
using Godot;
using Khepri.Descriptions;

namespace Khepri.Entities.Components
{
    /// <summary> A component that decorates its entity's name with a single adjective. The concrete kind <em>is</em> the adjective's category, and its place in <see cref="RoyalOrder"/> fixes where the word sits before the noun. </summary>
    /// <remarks> Modelling each adjective category as its own component (a material, a colour, a size) means there is no separate taxonomy to keep in step with the components — the components are the taxonomy. The English "royal order of adjectives" is then simply the order of these kinds. </remarks>
    [GlobalClass]
    public abstract partial class AdjectiveComponent : Component
    {
        /// <summary> The adjective component kinds in the order they read before a noun — opinion words first, material words last. A kind absent from this list sits nearest the noun. </summary>
        private static readonly Type[] RoyalOrder =
        {
            typeof(ConditionComponent),   // opinion
            typeof(MaterialComponent),    // material
        };


        /// <summary> The adjective this component contributes, for example <c>"iron"</c>. </summary>
        [Export] public String Word { get; set; } = String.Empty;


        /// <summary> This kind's position in the royal order; later positions sit closer to the noun. </summary>
        public Int32 RoyalIndex
        {
            get
            {
                Int32 index = Array.IndexOf(RoyalOrder, GetType());
                return index >= 0 ? index : RoyalOrder.Length;
            }
        }


        /// <inheritdoc/>
        public override void Contribute(NameBuilder builder)
        {
            builder.Adjective(Word);
        }
    }
}
