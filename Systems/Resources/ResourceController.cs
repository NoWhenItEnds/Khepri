using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Khepri.Nodes.Singletons;

namespace Khepri.Resources
{
    /// <summary> The central controller for all the game's entity resources. </summary>
    public partial class ResourceController : SingletonNode<ResourceController>
    {
        /// <summary> All the available resources loaded into memory. </summary>
        private Resource[] _resources;

        /// <inheritdoc/>
        public override void _Ready()
        {
            String[] filepaths = GetResources("res://Data", [".tres"]);

            _resources = new Resource[filepaths.Length];
            for (Int32 i = 0; i < _resources.Length; i++)
            {
                _resources[i] = ResourceLoader.Load(filepaths[i]);
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


        /// <summary> Create an instance of an entity resource of the given kind. </summary>
        /// <typeparam name="T"> The type of resource to generate. </typeparam>
        /// <param name="id"> The specific id or common name of the resource. </param>
        /// <returns> The generated resource, or a null if one couldn't be found. </returns>
        public T? CreateResource<T>(String id) where T : EntityResource
        {
            Resource? resource = _resources.FirstOrDefault(x => x is T entity && entity.Id == id) ?? null;
            if (resource != null) { return (T)resource.Duplicate(); }
            else { return null; }
        }
    }
}
