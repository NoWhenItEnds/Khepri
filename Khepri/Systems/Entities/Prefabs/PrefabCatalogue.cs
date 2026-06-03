using System;
using Khepri.Entities.Components;
using Khepri.Prefabs;

namespace Khepri.Entities.Prefabs
{
    /// <summary> A runtime store of named entity prefabs, populated at startup by scanning a directory of JSON files and accessible by name during play. </summary>
    /// <remarks>
    /// Thin domain subclass of <see cref="Catalogue{TOwner,TPart}"/> that pre-wires the <see cref="PrefabLoader{TOwner,TPart}"/> with the <c>"components"</c> array property name.
    /// Not thread-safe — populate fully at the composition root before any entity is created. Prefab names must be unique across all files in the directory; a duplicate triggers an explicit failure that names both files.
    /// </remarks>
    public sealed class PrefabCatalogue : Catalogue<Entity, Component>
    {
        /// <summary> Initialises the catalogue with the registry that will resolve component type keys when loading prefab files. </summary>
        /// <param name="registry"> The component registry injected from the composition root. </param>
        public PrefabCatalogue(ComponentRegistry registry)
            : base(new PrefabLoader<Entity, Component>(registry, "components"))
        {
        }
    }
}
