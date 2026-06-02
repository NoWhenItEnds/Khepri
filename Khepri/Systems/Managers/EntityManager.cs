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
            RegisterComponents(_componentRegistry);

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


        /// <summary> Register all the found components to the given registry. </summary>
        /// <param name="registry"> The initialised instance of a registry to register components into. </param>
        private void RegisterComponents(ComponentRegistry registry)
        {
            // TODO - Is there a way to use reflection to get all the components? Like how controllers are registered? Then we just strip the "Component" part?
            registry.Register("Health", (e, d) => new HealthComponent(e, d.GetInt32("max")));
            registry.Register("Head", (e, d) => new HeadComponent(e));
            registry.Register("Skill", (e, d) => new SkillComponent(e));
        }
    }
}
