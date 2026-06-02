using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Khepri.Entities.Prefabs
{
    /// <summary> A runtime store of named prefabs, populated at startup by scanning a directory of JSON files and accessible by name during play. </summary>
    /// <remarks> Not thread-safe — populate fully at the composition root before any entity is created. Each JSON file must conform to the schema defined in <see cref="PrefabLoader"/>. Prefab names must be unique across all files in the directory; a duplicate triggers an explicit failure that names both files. </remarks>
    public sealed class PrefabCatalogue
    {
        /// <summary> The loader used to parse each JSON file into a <see cref="LoadedPrefab"/>. </summary>
        private readonly PrefabLoader _loader;

        /// <summary> Maps each registered prefab name to its prefab, populated by <see cref="LoadDirectory"/>. </summary>
        private readonly Dictionary<String, EntityPrefab> _prefabs;

        /// <summary> Maps each registered prefab name to the file it was loaded from, used to produce helpful duplicate-name error messages. </summary>
        private readonly Dictionary<String, String> _sourceFiles;


        /// <summary> Initialises the catalogue with the registry that will be used to resolve component type keys when loading prefab files. </summary>
        /// <param name="registry"> The component registry injected from the composition root. Forwarded to a  <see cref="PrefabLoader"/> internally. </param>
        public PrefabCatalogue(ComponentRegistry registry)
        {
            ArgumentNullException.ThrowIfNull(registry);

            _loader      = new PrefabLoader(registry);
            _prefabs     = new Dictionary<String, EntityPrefab>();
            _sourceFiles = new Dictionary<String, String>();
        }


        /// <summary> Scans <paramref name="directoryPath"/> for <c>*.json</c> files, parses each as a prefab, and registers them by name. </summary>
        /// <remarks> Files are processed in the order returned by <see cref="Directory.GetFiles"/>. Parse failures for individual files are wrapped with the file path as context and re-thrown, halting the load — all files must be valid for the catalogue to be usable. </remarks>
        /// <param name="directoryPath"> The path to the directory containing prefab JSON files. </param>
        /// <exception cref="DirectoryNotFoundException"> Thrown when <paramref name="directoryPath"/> does not exist. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when two files declare the same prefab name (message names both files), or when a file fails schema validation, wrapped with the file path as context. </exception>
        /// <exception cref="System.Text.Json.JsonException"> Thrown when a file contains invalid JSON, wrapped with the file path as context. </exception>
        /// <exception cref="IOException"> Thrown when a file cannot be read, wrapped with the file path as context. </exception>
        public void LoadDirectory(String directoryPath)
        {
            Boolean exists = Directory.Exists(directoryPath);

            if (!exists)
            {
                throw new DirectoryNotFoundException($"Prefab directory '{directoryPath}' does not exist.");
            }

            String[] files = Directory.GetFiles(directoryPath, "*.json");

            foreach (String file in files)
            {
                LoadFile(file);
            }
        }


        /// <summary> Returns the prefab registered under <paramref name="name"/>. </summary>
        /// <param name="name"> The prefab name, as declared in its JSON <c>name</c> field. </param>
        /// <returns> The matching <see cref="EntityPrefab"/>. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when no prefab with <paramref name="name"/> has been loaded. The message lists the name so the caller can distinguish a typo from a missing file. </exception>
        public EntityPrefab Get(String name)
        {
            Boolean found = _prefabs.TryGetValue(name, out EntityPrefab? prefab);

            if (!found)
            {
                throw new KeyNotFoundException($"No prefab named '{name}' has been loaded into the catalogue.");
            }

            return prefab!;
        }


        /// <summary> Attempts to retrieve the prefab registered under <paramref name="name"/> without throwing when it has not been loaded. </summary>
        /// <param name="name"> The prefab name to look up. </param>
        /// <param name="prefab"> When this method returns <c>true</c>, the matching prefab; otherwise <c>null</c>. </param>
        /// <returns> <c>true</c> if the prefab was found; <c>false</c> otherwise. </returns>
        public Boolean TryGet(String name, out EntityPrefab? prefab) => _prefabs.TryGetValue(name, out prefab);


        /// <summary> Parses a single JSON file, checks for name collisions, and registers the resulting prefab. Re-throws IO, JSON, and schema failures from <see cref="PrefabLoader.Load"/> unchanged — they already carry the file path as context. </summary>
        /// <param name="filePath"> The path to the file being loaded. </param>
        /// <exception cref="InvalidOperationException"> Thrown when the file's prefab name is already registered; the message names both the already-loaded file and the new file. </exception>
        private void LoadFile(String filePath)
        {
            LoadedPrefab loaded = _loader.Load(filePath);

            Boolean duplicate = _sourceFiles.TryGetValue(loaded.Name, out String? existingFile);

            if (duplicate)
            {
                throw new InvalidOperationException($"Duplicate prefab name '{loaded.Name}': already loaded from '{existingFile}', conflict found in '{filePath}'.");
            }

            _prefabs[loaded.Name]     = loaded.Prefab;
            _sourceFiles[loaded.Name] = filePath;
        }
    }
}
