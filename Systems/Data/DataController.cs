using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using Khepri.Data.Actors;
using Khepri.Data.Celestial;
using Khepri.Data.Devices;
using Khepri.Data.Items;
using Khepri.Nodes.Singletons;
using Khepri.Types.Exceptions;

namespace Khepri.Data
{
    /// <summary> The central controller for all the game's entity resources. </summary>
    public partial class DataController : SingletonNode<DataController>
    {
        /// <summary> A mapping of kind to filepath for all the actor resources. </summary>
        private Dictionary<String, String> _actorData = new Dictionary<String, String>();

        /// <summary> A mapping of kind to filepath for all the stellar resources. </summary>
        private Dictionary<String, String> _celestialData = new Dictionary<String, String>();

        /// <summary> A mapping of kind to filepath for all the device resources. </summary>
        private Dictionary<String, String> _deviceData = new Dictionary<String, String>();

        /// <summary> A mapping of kind to filepath for all the item resources. </summary>
        private Dictionary<String, String> _itemData = new Dictionary<String, String>();

        /// <summary> An array of the currently loaded data objects. Allows for a lookup. </summary>
        private HashSet<EntityData> _loadedData = new HashSet<EntityData>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            foreach (String actor in GetResources("res://Data/Actors", [".json"]))
            {
                _actorData.Add(actor.GetFile().Split('.')[0], actor);
            }

            foreach (String celestial in GetResources("res://Data/Celestial", [".json"]))
            {
                _celestialData.Add(celestial.GetFile().Split('.')[0], celestial);
            }

            foreach (String device in GetResources("res://Data/Devices", [".json"]))
            {
                _deviceData.Add(device.GetFile().Split('.')[0], device);
            }

            foreach (String item in GetResources("res://Data/Items", [".json"]))
            {
                _itemData.Add(item.GetFile().Split('.')[0], item);
            }
        }


        /// <summary> Search the given directory, recursively, for files. </summary>
        /// <param name="directoryPath"> The Godot filepath to seach. </param>
        /// <param name="extensions"> A list of allowed extensions. A null means to accept everything. </param>
        /// <returns> The filepaths of all the found extensions. </returns>
        /// <exception cref="DirectoryNotFoundException"> If the given directory isn't found. </exception>
        private String[] GetResources(String directoryPath, String[]? extensions = null)
        {
            DirAccess dataDirectory = DirAccess.Open(directoryPath);
            if (dataDirectory == null)
            {
                throw new DirectoryNotFoundException($"The '{directoryPath}' directory does not exist! Something has gone very wrong Captain.");
            }

            List<String> resources = new List<String>();
            dataDirectory.ListDirBegin();
            String current = dataDirectory.GetNext();
            while (!String.IsNullOrEmpty(current))
            {
                String currentPath = directoryPath + '/' + current;
                if (dataDirectory.CurrentIsDir())
                {
                    resources.AddRange(GetResources(currentPath, extensions));
                }
                else
                {
                    // Only add the file if it's part of the acceptable extensions.
                    if (extensions == null || extensions.Contains(Path.GetExtension(currentPath)))
                    {
                        resources.Add(currentPath);
                    }
                }
                current = dataDirectory.GetNext();
            }
            dataDirectory.ListDirEnd();

            return resources.ToArray();
        }


        /// <summary> Create an instance of an entity data of the given kind. </summary>
        /// <typeparam name="T"> The type of resource to generate. </typeparam>
        /// <param name="kind"> The specific kind or common name of the data object. </param>
        /// <returns> The generated data object. </returns>
        /// <exception cref="DataException"> If the given kind or type couldn't be found. </exception>
        public T CreateEntityData<T>(String kind) where T : EntityData
        {
            String? filepath = null;
            switch (typeof(T))
            {
                case Type t when t == typeof(ActorData):
                    _actorData.TryGetValue(kind, out filepath);
                    break;
                case Type t when t == typeof(CelestialData):
                    _celestialData.TryGetValue(kind, out filepath);
                    break;
                case Type t when t == typeof(DeviceData):
                    _deviceData.TryGetValue(kind, out filepath);
                    break;
                case Type t when t == typeof(ItemData):
                    _itemData.TryGetValue(kind, out filepath);
                    break;
                default:
                    throw new DataException($"The given data type, {typeof(T)}, isn't supported.");
            }

            if (filepath == null)
            {
                throw new DataException($"Unable to create a new instance of the resource with the id {kind}.");
            }

            return JsonSerializer.Deserialize<T>(filepath) ?? throw new DataException($"Unable to create a new instance of the resource with the id {kind}.");
        }


        /// <summary> Register a loaded data object to the controller. </summary>
        /// <param name="data"> The data object to add. </param>
        /// <returns> Whether the data was added. </returns>
        public Boolean RegisterData(EntityData data) => _loadedData.Add(data);


        /// <summary> Attempt to get a loaded data object with the given uid. </summary>
        /// <param name="uid"> The unique id to search for. </param>
        /// <returns> The discovered data object. </returns>
        /// <exception cref="DataException"> If a resource with the id doesn't exist. </exception>
        public EntityData TryGetData(Guid uid)
        {
            EntityData? data = _loadedData.FirstOrDefault(x => x.UId == uid) ?? null;
            if (data == null)
            {
                throw new DataException($"Unable to find an instance of the resource with the id {uid}.");
            }
            return data;
        }


        /// <summary> Get all the data of a given type. </summary>
        /// <typeparam name="T"> The type of entity data object. </typeparam>
        /// <returns> An array of all the loaded data of the given type. </returns>
        public T[] GetDataType<T>() where T : EntityData => _loadedData.OfType<T>().ToArray();
    }
}
