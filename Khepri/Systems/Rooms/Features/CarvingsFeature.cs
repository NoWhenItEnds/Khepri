using Godot;
using Khepri.Descriptions;

namespace Khepri.Rooms.Features
{
    /// <summary> Carvings worked into the room's surfaces; reads as a hoverable note whose tooltip reveals what they depict. </summary>
    [GlobalClass]
    public partial class CarvingsFeature : Feature
    {
        /// <summary> Weaves the carvings into the room's prose as a hoverable note pointing back at this feature. </summary>
        /// <param name="builder"> The builder assembling the owning room's description. </param>
        public override void Contribute(DescriptionBuilder builder)
        {
            builder.Text("Ancient ").Note("carvings", this).Text(" spiral across every surface.");
        }
    }
}
