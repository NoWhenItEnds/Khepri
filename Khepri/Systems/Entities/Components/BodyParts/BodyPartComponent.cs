using Godot;

namespace Khepri.Entities.Components
{
    /// <summary> A kind of body part that an entity possesses. </summary>
    [GlobalClass]
    public abstract partial class BodyPartComponent : Component
    {
        // TODO - Add equipment slots. These will give access to wearing clothes, which have inventory components.

        // TODO - Some body parts also have inherent inventory slots.
    }
}
