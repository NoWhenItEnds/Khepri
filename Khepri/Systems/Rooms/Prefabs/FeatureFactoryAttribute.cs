using System;
using Khepri.Prefabs;

namespace Khepri.Rooms.Prefabs
{
    /// <summary> Marks a static method as the factory entry point for a room feature type, enabling <see cref="FeatureDiscovery"/> to register it automatically. </summary>
    /// <remarks> The decorated method must be <c>static</c>, accept <c>(Room, PrefabData)</c>, and return a type assignable to <see cref="Khepri.Rooms.Features.Feature"/>. Wrong parameter types or a non-assignable return type cause <see cref="FeatureDiscovery.RegisterAll"/> to throw at startup. Placing this attribute on a non-static method is silently skipped — only static methods are scanned. </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class FeatureFactoryAttribute : PrefabFactoryAttribute
    {
        /// <summary> Initialises the attribute, optionally pinning a specific type-key. </summary>
        /// <param name="typeKey"> The type-key to register under. Pass <c>null</c> or omit to derive the key from the declaring class name. </param>
        public FeatureFactoryAttribute(String? typeKey = null) : base(typeKey)
        {
        }
    }
}
