using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Entities.Prefabs;
using Khepri.Prefabs;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing the game's entities. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> The directory paths to search entity prefab definitions. </summary>
        /// <remarks> Paths must be relative Godot resource paths (e.g. <c>res://Khepri/Data/Prefabs/Entities</c>). All JSON files in each directory are scanned; prefab names must be unique across all files. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>();


        /// <summary> A cached reference to the component registry used by the prefab catalogue. </summary>
        private ComponentRegistry _componentRegistry = null!;

        /// <summary> The reverse type-to-key map used by the serialiser to emit the <c>"type"</c> field for each component. </summary>
        private TypeKeyMap _typeKeyMap = null!;

        /// <summary> A cached reference to the prefab catalogue. </summary>
        private PrefabCatalogue _prefabCatalogue = null!;

        /// <summary> The populator used by <see cref="CreateEntityFromPrefab"/> to recursively fill container components after the root entity is built bare. </summary>
        private EntityPopulator _populator = null!;

        /// <summary> All entities currently registered in the game world. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();


        /// <summary> Initialises the component registry, type-key map, prefab catalogue, and populator; loads all prefab JSON files from the configured paths. </summary>
        public override void _Ready()
        {
            _componentRegistry = new ComponentRegistry();
            _typeKeyMap        = new TypeKeyMap();
            ComponentDiscovery.RegisterAll(_componentRegistry, _typeKeyMap);

            _prefabCatalogue = new PrefabCatalogue(_componentRegistry);
            foreach (String path in _prefabPaths)
            {
                _prefabCatalogue.LoadDirectory(ProjectSettings.GlobalizePath(path));
            }

            _populator = new EntityPopulator(BareSpawnEntity);
        }


        /// <summary> Creates a fully populated entity from the named prefab, recursively instantiating any inventory contents declared in the prefab data. </summary>
        /// <remarks>
        /// Population is performed by <see cref="EntityPopulator"/>, which calls <see cref="BareSpawnEntity"/> for each child prefab name found in an <see cref="IPopulatableContainer"/>.
        /// An <see cref="InvalidOperationException"/> from the populator (prefab-name cycle) is intentionally allowed to propagate — a cycle indicates an authoring error that must be surfaced immediately.
        /// This method must not be used for the reconstruct path; <c>EntityReconstructor</c> attaches saved children itself and must not re-queue prefab contents.
        /// </remarks>
        /// <param name="prefabName"> The name of the prefab to instantiate; must exist in the loaded catalogue. </param>
        /// <returns> A fully populated entity with all nested container contents instantiated. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when a prefab-name expansion cycle is detected, or when the resulting entity's UID is already registered (which would indicate an identity bug). </exception>
        public Entity CreateEntityFromPrefab(String prefabName)
        {
            Entity root = BareSpawnEntity(prefabName);
            _populator.Populate(root, prefabName);

            Boolean isAdded = _entities.Add(root);
            if (!isAdded)
            {
                throw new InvalidOperationException(
                    $"Entity with UID '{root.UId}' (prefab '{prefabName}') could not be registered because that UID already exists in the entity set. This indicates an identity collision bug.");
            }

            return root;
        }


        /// <summary> Builds a single, unpopulated entity from its prefab — components are attached but no container contents are instantiated. </summary>
        /// <remarks>
        /// This is the bare-spawn delegate passed to <see cref="EntityPopulator"/>; it must not call <see cref="CreateEntityFromPrefab"/> to avoid re-entrant population.
        /// Only the root entity passed to <see cref="CreateEntityFromPrefab"/> is registered in <c>_entities</c>; nested children produced via this method are intentionally not tracked directly and are reachable only through container traversal on their ancestor.
        /// </remarks>
        /// <param name="prefabName"> The name of the prefab to build. </param>
        /// <returns> An entity with its prefab components attached and pending container contents recorded, but not yet populated. </returns>
        private Entity BareSpawnEntity(String prefabName)
        {
            return EntityFactory.CreateFrom(_prefabCatalogue.Get(prefabName)).Build();
        }
    }
}
