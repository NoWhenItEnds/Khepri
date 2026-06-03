using System;

namespace Khepri.Prefabs
{
    /// <summary> The result of parsing a single prefab JSON file: the prefab's declared name and its fully constructed <see cref="Prefab{TOwner,TPart}"/> instance, ready to be keyed into a catalogue. </summary>
    /// <typeparam name="TOwner"> The type of the object built from the prefab; must implement <see cref="IPartContainer{TPart}"/>. </typeparam>
    /// <typeparam name="TPart"> The base type of parts stored in the prefab. </typeparam>
    /// <param name="Name"> The prefab's declared name, sourced from the <c>name</c> field of the JSON document. </param>
    /// <param name="Prefab"> The prefab ready to be applied to new owners via a domain-specific factory. </param>
    public sealed record LoadedPrefab<TOwner, TPart>(String Name, Prefab<TOwner, TPart> Prefab)
        where TOwner : IPartContainer<TPart>;
}
