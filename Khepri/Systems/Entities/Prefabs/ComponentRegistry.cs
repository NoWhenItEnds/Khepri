using Khepri.Entities.Components;
using Khepri.Prefabs;

namespace Khepri.Entities.Prefabs
{
    /// <summary> Maps string type-keys to component factories; the Open/Closed extension point for runtime prefab loading. </summary>
    /// <remarks> Thin domain subclass of <see cref="FactoryRegistry{TOwner,TPart}"/> that fixes the type parameters to <see cref="Entity"/> and <see cref="Component"/>, so callers never need to repeat those type arguments. All registration and resolution behaviour is inherited from the generic base. </remarks>
    public sealed class ComponentRegistry : FactoryRegistry<Entity, Component>
    {
    }
}
