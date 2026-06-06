using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Extensions.Logging;
using Jaypen.Utilities.Extensions;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Entities.Components;
using Khepri.Entities.Controllers;

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

        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<EntityManager>();


        /// <inheritdoc/>
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


        /// <summary> Instantiates the given prefab, wires an <see cref="AiController"/>, registers the entity in the game world, and returns it. </summary>
        /// <param name="prefab"> The prefab to instantiate; must not be null. </param>
        /// <returns> The newly constructed, registered entity. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the new entity's UID collides with one already registered (an identity bug). </exception>
        public Entity CreateEntity(EntityPrefab prefab)
        {
            Entity entity = prefab.Instantiate();
            AiController controller = new AiController(entity); // TODO - Not happy about always being AiController.
            Boolean added = _entities.TryAdd(entity, controller);

            if (!added)
            {
                throw new InvalidOperationException(
                    $"Entity with UID '{entity.UId}' (prefab '{prefab.Name}') could not be registered because that UID already exists. This indicates an identity collision bug.");
            }

            return entity;
        }


        /// <summary> Resolves a prefab by name and delegates to <see cref="CreateEntity(EntityPrefab)"/> to build and register the entity. </summary>
        /// <param name="prefabName"> The name of the prefab to instantiate; must exist in a loaded directory. </param>
        /// <returns> The newly constructed, registered entity. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when no prefab with <paramref name="prefabName"/> has been loaded. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the new entity's UID collides with one already registered (an identity bug). </exception>
        public Entity CreateEntity(String prefabName)
        {
            Boolean found = _prefabsByName.TryGetValue(prefabName, out EntityPrefab? prefab);

            if (!found)
            {
                throw new KeyNotFoundException($"No entity prefab named '{prefabName}' has been loaded.");
            }

            return CreateEntity(prefab!);
        }


        /// <summary> Registers a single prefab by its name, rejecting blanks and duplicates. </summary>
        /// <summary> Registers a single entity prefab by its name, after verifying its authored components. </summary>
        /// <remarks> Any single-file authoring slip — a blank name, a duplicate name, or a component that fails validation — is logged and skipped rather than crashing the boot, so one bad file reports loudly without taking the whole game down and every other prefab still loads. The first prefab registered under a name wins. </remarks>
        /// <param name="prefab"> The loaded prefab to register. </param>
        /// <param name="resourcePath"> The resource path, used to enrich the skip log. </param>
        private void Register(EntityPrefab prefab, String resourcePath)
        {
            if (String.IsNullOrWhiteSpace(prefab.Name))
            {
                Logger.LogError("Skipping entity prefab '{Path}': it has a blank Name.", resourcePath);
            }
            else if (_prefabsByName.ContainsKey(prefab.Name))
            {
                Logger.LogError("Skipping duplicate entity prefab name '{Name}' (from '{Path}'); keeping the one already registered.", prefab.Name, resourcePath);
            }
            else if (ComponentsValid(prefab, resourcePath))
            {
                _prefabsByName[prefab.Name] = prefab;
            }
        }


        /// <summary> Verifies every authored component on the prefab up front, so authoring mistakes surface at boot rather than when an entity first spawns. </summary>
        /// <remarks> A component that fails validation is logged and reported as invalid rather than throwing, so one bad prefab is skipped without taking the whole boot down. </remarks>
        /// <param name="prefab"> The prefab whose components are validated. </param>
        /// <param name="resourcePath"> The resource path, used to enrich the skip log. </param>
        /// <returns> <c>true</c> when every component validates; <c>false</c> when any one fails. </returns>
        private static Boolean ComponentsValid(EntityPrefab prefab, String resourcePath)
        {
            Boolean valid = true;

            try
            {
                foreach (Component component in prefab.Components)
                {
                    component.Validate(prefab);
                }
            }
            catch (Exception error)
            {
                Logger.LogError(error, "Skipping entity prefab '{Name}' (from '{Path}'): it failed component validation.", prefab.Name, resourcePath);
                valid = false;
            }

            return valid;
        }
    }
}
