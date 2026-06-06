using Godot;

namespace Khepri.Entities.Components.Parts
{
    /// <summary> A part representing one head possessed by an entity. </summary>
    /// <remarks>
    /// A head is pure anatomy; its <see cref="PartComponent.Kind"/> is typically <c>null</c> unless a head-specific <see cref="Khepri.Entities.EntityKind"/> variant is introduced in future.
    /// TODO: Add equipment slots. These will give access to wearing clothes, which have inventory components.
    /// TODO: Some body parts also have inherent inventory slots.
    /// </remarks>
    [GlobalClass]
    public partial class HeadPart : PartComponent
    {
    }
}
