using System;
using System.Collections.Generic;
using Godot;
using Jaypen.Utilities.Extensions;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Entities.Controllers;
using Khepri.Entities.Definitions;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing the game's entities. </summary>
    public partial class EntityManager : SingletonNode<EntityManager>
    {
        /// <summary> The Godot resource directories to scan for entity prefab (<c>*.tres</c>) definitions. </summary>
        /// <remarks> Paths are Godot resource paths (e.g. <c>res://Khepri/Data/Prefabs/Entities</c>). Every <c>.tres</c> in each directory is loaded as an <see cref="EntityPrefab"/>; prefab names must be unique across all directories. </remarks>
        [ExportGroup("Settings")]
        [Export] private Godot.Collections.Array<String> _prefabPaths = new Godot.Collections.Array<String>
        {
            "res://Khepri/Data/Prefabs/Entities"
        };


        /// <summary> All loaded entity prefabs, keyed by their <see cref="EntityPrefab.Name"/>. </summary>
        private readonly Dictionary<String, EntityPrefab> _prefabsByName = new Dictionary<String, EntityPrefab>();

        /// <summary> All entities currently registered in the game world and their associated controller (if they have one). </summary>
        /// <remarks> A null controller indicates that the entity doesn't currently have an associated controller. </remarks>
        private readonly Dictionary<Entity, EntityController?> _entities = new Dictionary<Entity, EntityController?>();


        /// <summary> Loads every entity prefab resource from the configured directories and registers it by name. </summary>
        public override void _Ready()
        {
            foreach (String path in _prefabPaths)
            {
                foreach (EntityPrefab prefab in ResourceExtensions.GetResources<EntityPrefab>(path))
                {
                    Register(prefab, prefab.ResourcePath);
                }
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
            AiController controller = new AiController(entity); // TODO - Not happy about always being AiController.
            Boolean added  = _entities.TryAdd(entity, controller);

            if (!added)
            {
                throw new InvalidOperationException(
                    $"Entity with UID '{entity.UId}' (prefab '{prefabName}') could not be registered because that UID already exists. This indicates an identity collision bug.");
            }

            return entity;
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
