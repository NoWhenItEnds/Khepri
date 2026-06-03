using System;
using Khepri.Prefabs;
using Khepri.Rooms.Features;

namespace Khepri.Rooms.Prefabs
{
    /// <summary> A runtime store of named room prefabs, populated at startup by scanning a directory of JSON files and accessible by name during play. </summary>
    /// <remarks>
    /// Thin domain subclass of <see cref="Catalogue{TOwner,TPart}"/> that pre-wires the <see cref="PrefabLoader{TOwner,TPart}"/> with the <c>"features"</c> array property name.
    /// Not thread-safe — populate fully at the composition root before any room is created. Prefab names must be unique across all files in the directory; a duplicate triggers an explicit failure that names both files.
    /// </remarks>
    public sealed class RoomCatalogue : Catalogue<Room, Feature>
    {
        /// <summary> Initialises the catalogue with the registry that will resolve feature type keys when loading prefab files. </summary>
        /// <param name="registry"> The room feature registry injected from the composition root. </param>
        public RoomCatalogue(FeatureRegistry registry)
            : base(new PrefabLoader<Room, Feature>(registry, "features"))
        {
        }
    }
}
