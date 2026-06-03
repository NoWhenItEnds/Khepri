using System;
using Khepri.Prefabs;

namespace Khepri.Entities.Prefabs
{
    /// <summary> Marks a static method as the factory entry point for a component type, enabling <see cref="ComponentDiscovery"/> to register it automatically. </summary>
    /// <remarks> The decorated method must be <c>static</c>, accept <c>(Entity, PrefabData)</c>, and return a type assignable to <see cref="Khepri.Entities.Components.Component"/>. Wrong parameter types or a non-assignable return type cause <see cref="ComponentDiscovery.RegisterAll"/> to throw at startup. Placing this attribute on a non-static method is silently skipped — only static methods are scanned. </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ComponentFactoryAttribute : PrefabFactoryAttribute
    {
        /// <summary> Initialises the attribute, optionally pinning a specific type-key. </summary>
        /// <param name="typeKey"> The type-key to register under. Pass <c>null</c> or omit to derive the key from the declaring class name. </param>
        public ComponentFactoryAttribute(String? typeKey = null) : base(typeKey)
        {
        }
    }
}
