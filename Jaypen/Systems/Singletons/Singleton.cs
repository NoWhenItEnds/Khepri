using System;
using Godot;

namespace Jaypen.Utilities.Singletons
{
    /// <summary> Provides the shared singleton lifecycle logic for all Godot singleton base classes. </summary>
    /// <typeparam name="T">The concrete singleton type whose instance is managed.</typeparam>
    /// <remarks> Because C# prohibits deriving from a type parameter, the implementation is factored into static methods called by each alias class rather than being expressed as a generic base class. </remarks>
    internal static class SingletonBehaviour<T> where T : Node
    {
        /// <summary> The single live instance of <typeparamref name="T"/>, or <c>null</c> if no instance has entered the scene tree yet. </summary>
        internal static T? Instance { get; private set; }

        /// <summary> Attempts to register <paramref name="candidate"/> as the singleton instance. </summary>
        /// <param name="candidate"> The node entering the scene tree. </param>
        /// <returns> <c>true</c> when the candidate became the registered instance. Returns <c>false</c> if a duplicate was detected and the caller should invoke <c>QueueFree()</c> on itself. </returns>
        internal static Boolean TryRegister(T candidate)
        {
            Boolean registered;

            if (Instance == null)
            {
                Instance = candidate;
                registered = true;
            }
            else
            {
                GD.PushWarning($"[Singleton] Duplicate instance of {typeof(T).Name} detected.");
                registered = false;
            }

            return registered;
        }

        /// <summary> Clears the singleton reference if <paramref name="exiting"/> is the current instance, preserving any replacement that may already have taken over. </summary>
        /// <param name="exiting">The node that is leaving the scene tree.</param>
        internal static void Unregister(T exiting)
        {
            if (Instance == exiting)
            {
                Instance = null;
            }
        }
    }
}
