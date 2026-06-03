using Godot;
using Khepri.Rooms.Features;

namespace Khepri.Rooms.Definitions
{
    /// <summary> The authored, designer-editable definition of a single room feature, used by <see cref="RoomPrefab"/> to construct a live <see cref="Feature"/> for a freshly built room. </summary>
    /// <remarks> The room-side counterpart to <c>ComponentDef</c>: concrete subclasses are <c>[GlobalClass]</c> resources edited in the Inspector, while the runtime <see cref="Feature"/> types stay plain POCOs. </remarks>
    [GlobalClass]
    public abstract partial class FeatureDef : Resource
    {
        /// <summary> Constructs a live feature for <paramref name="owner"/> from this definition's authored values. </summary>
        /// <param name="owner"> The room the produced feature will belong to. </param>
        /// <returns> A fully constructed, non-null <see cref="Feature"/>. </returns>
        public abstract Feature Create(Room owner);
    }
}
