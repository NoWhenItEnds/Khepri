using System;
using System.Collections.Generic;

namespace Khepri.Rooms
{
    /// <summary> An immutable specification for a single room instance within a world definition: its unique instance identifier, the prefab template to apply, and the entity prefab names to spawn into it. </summary>
    /// <param name="Id"> The instance identifier that uniquely names this room within the world definition; used as the key in connection specs. </param>
    /// <param name="PrefabName"> The name of the room prefab to apply when constructing this room instance. </param>
    /// <param name="EntityPrefabNames"> The ordered list of entity prefab names to spawn inside this room instance; may be empty when no entities are specified. </param>
    public sealed record RoomInstanceSpec(
        String              Id,
        String              PrefabName,
        IReadOnlyList<String> EntityPrefabNames);
}
