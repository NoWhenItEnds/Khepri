using Godot;
using Khepri.Descriptions;

namespace Khepri.Rooms.Features
{
    /// <summary> A standing brazier that lights the room; reads as a hoverable note whose tooltip describes it more closely. </summary>
    [GlobalClass]
    public partial class BrazierFeature : Feature
    {
        /// <summary> Weaves the brazier into the room's prose as a hoverable note pointing back at this feature. </summary>
        /// <param name="builder"> The builder assembling the owning room's description. </param>
        public override void Contribute(DescriptionBuilder builder)
        {
            builder.Text("A ").Note("bronze brazier", this).Text(" smoulders against the far wall, breathing dim orange light.");
        }
    }
}
