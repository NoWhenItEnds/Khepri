using System;
using Godot;
using Khepri.Rooms.Features;

namespace Khepri.Rooms.Definitions
{
    /// <summary> Authored definition of a <see cref="DescriptionFeature"/>: supplies the fixed descriptive text for a room. </summary>
    [GlobalClass]
    public partial class DescriptionDef : FeatureDef
    {
        /// <summary> The description text exposed to the player via <see cref="Room.BuildDescription"/>. Must not be blank. </summary>
        [Export(PropertyHint.MultilineText)] public String Text { get; set; } = String.Empty;


        /// <inheritdoc/>
        public override Feature Create(Room owner)
        {
            return new DescriptionFeature(owner, Text);
        }
    }
}
