using Godot;

namespace Khepri.Rooms.Features
{
    /// <summary> Carvings worked into the room's surfaces. </summary>
    /// <remarks> Carries no behaviour of its own: its room prose (with the carvings brace-marked as a hoverable note) is authored in <see cref="Feature.Prose"/>, and the note's tooltip in <see cref="Feature.Description"/>. </remarks>
    [GlobalClass]
    public partial class CarvingsFeature : Feature
    {
    }
}
