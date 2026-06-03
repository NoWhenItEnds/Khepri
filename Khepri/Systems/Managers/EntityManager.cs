using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Entities.Definitions;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing the game's entities. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> The Godot resource directories to scan for entity prefab (<c>*.tres</c>) definitions. </summary>
        /// <remarks> Paths are Godot resource paths (e.g. <c>res://Khepri/Data/Prefabs/Entities</c>). Every <c>.tres</c> in each directory is loaded as an <see cref="EntityPrefab"/>; prefab names must be unique across all directories. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>();


        /// <summary> All loaded entity prefabs, keyed by their <see cref="EntityPrefab.Name"/>. </summary>
        private readonly Dictionary<String, EntityPrefab> _prefabsByName = new Dictionary<String, EntityPrefab>();

        /// <summary> All entities currently registered in the game world. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();


        /// <summary> Loads every entity prefab resource from the configured directories and registers it by name. </summary>
        public override void _Ready()
        {
            foreach (String path in _prefabPaths)
            {
                LoadPrefabsFrom(path);
            }
        }


        /// <summary> Creates an entity from the named prefab and registers it in the world. </summary>
        /// <param name="prefabName"> The name of the prefab to instantiate; must exist in a loaded directory. </param>
        /// <returns> The newly constructed, registered entity. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when no prefab with <paramref name="prefabName"/> has been loaded. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the new entity's UID collides with one already registered (an identity bug). </exception>
        public Entity CreateEntityFromPrefab(String prefabName)
        {
            Boolean found = _prefabsByName.TryGetValue(prefabName, out EntityPrefab? prefab);

            if (!found)
            {
                throw new KeyNotFoundException($"No entity prefab named '{prefabName}' has been loaded.");
            }

            Entity  entity = prefab!.Instantiate();
            Boolean added  = _entities.Add(entity);

            if (!added)
            {
                throw new InvalidOperationException(
                    $"Entity with UID '{entity.UId}' (prefab '{prefabName}') could not be registered because that UID already exists. This indicates an identity collision bug.");
            }

            return entity;
        }


        /// <summary> Loads every <c>*.tres</c> entity prefab in a single directory and registers each by name. </summary>
        /// <param name="directory"> The Godot resource directory path to scan. </param>
        /// <exception cref="InvalidOperationException"> Thrown when the directory cannot be opened, a resource fails to load as an <see cref="EntityPrefab"/>, a prefab name is blank, or two prefabs share a name. </exception>
        private void LoadPrefabsFrom(String directory)
        {
            using DirAccess access = DirAccess.Open(directory);

            if (access is null)
            {
                throw new InvalidOperationException($"Entity prefab directory '{directory}' could not be opened (error {DirAccess.GetOpenError()}).");
            }

            foreach (String fileName in access.GetFiles())
            {
                Boolean isResource = fileName.EndsWith(".tres", StringComparison.OrdinalIgnoreCase);

                if (!isResource)
                {
                    continue;
                }

                String       resourcePath = $"{directory}/{fileName}";
                EntityPrefab prefab       = ResourceLoader.Load<EntityPrefab>(resourcePath)
                    ?? throw new InvalidOperationException($"Resource '{resourcePath}' could not be loaded as an EntityPrefab.");

                Register(prefab, resourcePath);
            }
        }


        /// <summary> Registers a single prefab by its name, rejecting blanks and duplicates. </summary>
        /// <param name="prefab"> The loaded prefab to register. </param>
        /// <param name="resourcePath"> The resource path, used only to enrich error messages. </param>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="EntityPrefab.Name"/> is blank or already registered. </exception>
        private void Register(EntityPrefab prefab, String resourcePath)
        {
            if (String.IsNullOrWhiteSpace(prefab.Name))
            {
                throw new InvalidOperationException($"Entity prefab '{resourcePath}' has a blank Name.");
            }

            Boolean duplicate = _prefabsByName.ContainsKey(prefab.Name);

            if (duplicate)
            {
                throw new InvalidOperationException($"Duplicate entity prefab name '{prefab.Name}' (from '{resourcePath}').");
            }

            _prefabsByName[prefab.Name] = prefab;
        }
    }
}
