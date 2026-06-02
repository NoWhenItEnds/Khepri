using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Jaypen.Utilities.Logging;

namespace Jaypen.Utilities.Extensions
{
    /// <summary> Helpful extensions for working with resources. </summary>
    public static class ResourceExtensions
    {
        /// <summary> Logger used to surface type-mismatch warnings without throwing. </summary>
        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => Log.For(nameof(ResourceExtensions)));


        /// <summary> Searches <paramref name="directoryPath"/> recursively for <c>.tres</c> files, loads each one, and returns those that are instances of <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The expected resource type; files of any other type are skipped with a warning. </typeparam>
        /// <param name="directoryPath"> Godot path of the root directory to search. </param>
        /// <returns> An array containing every loaded resource that is assignable to <typeparamref name="T"/>. </returns>
        public static T[] GetResources<T>(String directoryPath) where T : Resource
        {
            String[] resourcePaths = FileExtensions.GetFilepaths(directoryPath, [".tres"]);

            List<T> results = new List<T>();
            foreach (String path in resourcePaths)
            {
                Resource resource = ResourceLoader.Load(path);
                if (resource is T castResource)
                {
                    results.Add(castResource);
                }
                else
                {
                    Logger.Value.LogWarning("Skipping '{Path}': expected a resource of type {Expected} but found {Actual}. This is likely a misconfiguration — verify the file is the correct type.",
                        path,
                        typeof(T).FullName,
                        resource?.GetType().FullName ?? "null");
                }
            }

            return results.ToArray();
        }
    }
}
