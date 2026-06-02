using System;

namespace Khepri.Entities.Prefabs
{
    /// <summary> The result of parsing a single prefab JSON file: the prefab's declared name and its fully constructed <see cref="Prefab"/> instance, ready to be keyed into a catalogue. </summary>
    /// <param name="Name"> The prefab's declared name, sourced from the <c>name</c> field of the JSON document. </param>
    /// <param name="Prefab"> The prefab ready to be applied to new entities via <see cref="EntityFactory.CreateFrom"/>. </param>
    public sealed record LoadedPrefab(String Name, EntityPrefab Prefab);
}
