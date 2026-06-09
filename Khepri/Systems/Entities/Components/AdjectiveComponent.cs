using System;
using Godot;

namespace Khepri.Entities.Components
{
    /// <summary> A component that decorates its entity's name with a single adjective. The concrete kind <em>is</em> the adjective's category, and its place in <see cref="RoyalOrder"/> fixes where the word sits before the noun. </summary>
    [GlobalClass]
    public abstract partial class AdjectiveComponent : Component
    {
        /// <summary> The adjective component kinds in the order they read before a noun — opinion words first, material words last. A kind absent from this list sits nearest the noun. </summary>
        private static readonly Type[] RoyalOrder =
        {
            typeof(ConditionComponent),   // opinion
            typeof(MaterialComponent),    // material
        };


        /// <summary> This kind's position in the royal order; later positions sit closer to the noun. </summary>
        public Int32 RoyalIndex
        {
            get
            {
                Int32 index = Array.IndexOf(RoyalOrder, GetType());
                return index >= 0 ? index : RoyalOrder.Length;
            }
        }


        /// <summary> Gets the adjective this component contributes. </summary>
        /// <returns> The adjective as a string. </returns>
        public abstract String GetAdjective();
    }
}
