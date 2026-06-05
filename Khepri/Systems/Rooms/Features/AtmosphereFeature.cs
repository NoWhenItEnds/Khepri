using Godot;

namespace Khepri.Rooms.Features
{
    /// <summary> Ambient flavour that sets a room's scene — plain, non-interactive prose with no note of its own. </summary>
    /// <remarks> Relies entirely on the base <see cref="Feature"/>: its authored <see cref="Feature.Description"/> is contributed straight to the room as prose. Typically given a low <see cref="Feature.Order"/> so it opens the description. </remarks>
    [GlobalClass]
    public partial class AtmosphereFeature : Feature
    {
    }
}
