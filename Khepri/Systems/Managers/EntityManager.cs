using System;
using Godot;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Entities.Components;
using Khepri.Entities.Prefabs;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing the game's entities. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> The directory paths to search entity prefab definitions. </summary>
        /// <remarks> The should be relative, Godot paths. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>();

        /// <summary> A cached reference to the component registry. </summary>
        private ComponentRegistry _componentRegistry = null!;

        /// <summary> A cached reference to the prefab registry. </summary>
        private PrefabCatalogue _prefabCatalogue = null!;

        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<EntityManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            _componentRegistry = new ComponentRegistry();
            _componentRegistry.Register("Health", (e, d) => new HealthComponent(e, d.GetInt32("max")));
            _prefabCatalogue = new PrefabCatalogue(_componentRegistry);

            foreach (String path in _prefabPaths)
            {
                _prefabCatalogue.LoadDirectory(ProjectSettings.GlobalizePath(path));
            }
        }


        /// <summary> Create an instance of an entity prefab. </summary>
        /// <param name="prefabName"> The name of the prefab to load. </param>
        /// <returns> A reference to the generated entity. </returns>
        public Entity CreateEntityFromPrefab(String prefabName) => EntityFactory.CreateFrom(_prefabCatalogue.Get(prefabName)).Build();
    }
}
