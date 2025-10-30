using Godot;
using System;
using System.IO;

namespace Khepri.Types.Extensions
{
    /// <summary> Helpful extensions for working with system files and directories. </summary>
    public static class IOExtensions
    {
        /// <summary> Attempt to load the entity's world sprites from the filesystem. </summary>
        /// <returns> The loaded resource. </returns>
        /// <exception cref="FileNotFoundException"> If the file doesn't exist. </exception>
        /// <exception cref="InvalidCastException"> If the file is not of the correct type. </exception>
        public static T GetResource<T>(String filepath) where T : Resource
        {
            if (!ResourceLoader.Exists(filepath))
            {
                throw new FileNotFoundException($"No file exists at the filepath, {filepath}.");
            }
            T? result = ResourceLoader.Load<T>(filepath);
            if (result == null)
            {
                throw new InvalidCastException($"Unable to cast the loaded resource to a sprite frame.");
            }
            return result;
        }
    }
}
