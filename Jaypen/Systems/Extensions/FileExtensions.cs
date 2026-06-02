using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jaypen.Utilities.Extensions
{
    /// <summary> A helper class for working with files and filepaths. </summary>
    public static class FileExtensions
    {
        /// <summary> Search the given directory, recursively, for files. </summary>
        /// <param name="directoryPath"> The Godot filepath at which to begin the search. </param>
        /// <param name="extensions"> Set of file extensions (including the leading dot, e.g. <c>".tres"</c>) to accept. Pass <see langword="null"/> to accept all extensions. </param>
        /// <returns> The filepaths of all matched files in breadth-first order. </returns>
        /// <exception cref="DirectoryNotFoundException"> Thrown when <paramref name="directoryPath"/> cannot be opened by Godot. </exception>
        public static String[] GetFilepaths(String directoryPath, HashSet<String>? extensions = null)
        {
            using DirAccess? rootAccess = DirAccess.Open(directoryPath);
            if (rootAccess == null)
            {
                throw new DirectoryNotFoundException($"The '{directoryPath}' directory does not exist.");
            }

            List<String> results = new List<String>();
            HashSet<String> visited = new HashSet<String>(StringComparer.Ordinal);
            Stack<String> pending = new Stack<String>();
            pending.Push(directoryPath);

            while (pending.Count > 0)
            {
                String currentDir = pending.Pop();

                // Normalise the path to a canonical form so that symlinks to the same
                // directory from different paths are detected as already-visited.
                String normalisedDir = ProjectSettings.GlobalizePath(currentDir);
                Boolean alreadyVisited = !visited.Add(normalisedDir);

                if (!alreadyVisited)
                {
                    using DirAccess? dir = DirAccess.Open(currentDir);
                    if (dir != null)
                    {
                        dir.ListDirBegin();
                        String entry = dir.GetNext();
                        while (!String.IsNullOrEmpty(entry))
                        {
                            String entryPath = currentDir + "/" + entry;
                            if (dir.CurrentIsDir())
                            {
                                pending.Push(entryPath);
                            }
                            else
                            {
                                Boolean accepted = extensions == null
                                    || extensions.Contains(Path.GetExtension(entryPath));
                                if (accepted)
                                {
                                    results.Add(entryPath);
                                }
                            }

                            entry = dir.GetNext();
                        }

                        dir.ListDirEnd();
                    }
                }
            }

            return results.ToArray();
        }
    }
}
