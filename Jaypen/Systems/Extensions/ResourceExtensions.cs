using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Jaypen.Logging;

namespace Jaypen.Extensions
{
    /// <summary> Helpful extensions for working with resources. </summary>
    public static class ResourceExtensions
    {
        /// <summary> Logger used to surface type-mismatch warnings without throwing. </summary>
        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => Log.For(nameof(ResourceExtensions)));


        /// <summary> Attempts to load the resource at <paramref name="path"/> as <typeparamref name="T"/>. A missing file or mismatched type logs a warning and returns <see langword="false"/> rather than throwing, matching the forgiving style of <see cref="GetResources{T}"/>. </summary>
        /// <typeparam name="T"> The expected resource type. </typeparam>
        /// <param name="path"> Godot path of the resource to load. </param>
        /// <param name="result"> The loaded resource, or null when loading or the type check failed. </param>
        /// <returns> If the resource was loaded successfully. </returns>
        public static Boolean TryLoad<T>(String path, out T? result) where T : Resource
        {
            Resource? resource = ResourceLoader.Load(path);

            if (resource is T castResource)
            {
                result = castResource;
            }
            else
            {
                result = null;
                Logger.Value.LogWarning("Could not load '{Path}' as {Expected}: found {Actual}.",
                    path,
                    typeof(T).FullName,
                    resource?.GetType().FullName ?? "nothing (missing or unloadable file)");
            }

            return result != null;
        }


        /// <summary> Searches <paramref name="directoryPath"/> recursively for <c>.tres</c> files, loads each one, and returns those that are instances of <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The expected resource type; files of any other type are skipped with a warning. </typeparam>
        /// <param name="directoryPath"> Godot path of the root directory to search. </param>
        /// <returns> An array containing every loaded resource that is assignable to <typeparamref name="T"/>. </returns>
        /// <remarks> In exported builds Godot renames text resources inside the PCK to <c>&lt;name&gt;.tres.remap</c>, so discovery accepts both forms and strips the <c>.remap</c> suffix before loading — <see cref="ResourceLoader"/> resolves the original path through the remap table. </remarks>
        public static T[] GetResources<T>(String directoryPath) where T : Resource
        {
            String[] resourcePaths = FileExtensions.GetFilepaths(directoryPath, [".tres", ".remap"]);

            List<T> results = new List<T>();
            foreach (String path in resourcePaths)
            {
                Boolean isTres = path.EndsWith(".tres", StringComparison.Ordinal);
                Boolean isTresRemap = path.EndsWith(".tres.remap", StringComparison.Ordinal);

                // Non-.tres remaps (e.g. remapped scenes) fall outside this loader's contract and are ignored without a warning.
                if (isTres || isTresRemap)
                {
                    String loadPath = isTresRemap ? path[..^".remap".Length] : path;
                    Resource resource = ResourceLoader.Load(loadPath);
                    if (resource is T castResource)
                    {
                        results.Add(castResource);
                    }
                    else
                    {
                        Logger.Value.LogWarning("Skipping '{Path}': expected a resource of type {Expected} but found {Actual}. This is likely a misconfiguration — verify the file is the correct type.",
                            loadPath,
                            typeof(T).FullName,
                            resource?.GetType().FullName ?? "null");
                    }
                }
            }

            return results.ToArray();
        }
    }
}
