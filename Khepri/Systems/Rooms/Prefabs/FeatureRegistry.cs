using Khepri.Prefabs;
using Khepri.Rooms.Features;

namespace Khepri.Rooms.Prefabs
{
    /// <summary> Maps string type-keys to room feature factories; the Open/Closed extension point for runtime room prefab loading. </summary>
    /// <remarks> Thin domain subclass of <see cref="FactoryRegistry{TOwner,TPart}"/> that fixes the type parameters to <see cref="Room"/> and <see cref="Feature"/>, so callers never need to repeat those type arguments. All registration and resolution behaviour is inherited from the generic base. </remarks>
    public sealed class FeatureRegistry : FactoryRegistry<Room, Feature>
    {
    }
}
