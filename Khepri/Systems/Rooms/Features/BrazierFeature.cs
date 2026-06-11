using Godot;

namespace Khepri.Rooms.Features
{
    /// <summary> A standing brazier that lights the room. </summary>
    /// <remarks> Carries no behaviour of its own: its room prose (with the brazier brace-marked as a hoverable note) is authored in <see cref="Feature.Prose"/>, and the note's tooltip in <see cref="Feature.Description"/>. </remarks>
    [GlobalClass]
    public partial class BrazierFeature : Feature
    {
    }
}
