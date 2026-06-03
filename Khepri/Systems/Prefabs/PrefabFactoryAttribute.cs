using System;

namespace Khepri.Prefabs
{
    /// <summary> Base attribute that marks a static method as a factory entry point for a particular part type, enabling <see cref="FactoryDiscovery{TOwner,TPart}"/> to register it automatically. </summary>
    /// <remarks>
    /// Domain-specific attributes (such as <c>[ComponentFactory]</c>) subclass this attribute to let discovery scan for them specifically, keeping component and room-feature registries independent.
    /// The decorated method must be <c>static</c>, accept <c>(TOwner, PrefabData)</c>, and return a type assignable to <c>TPart</c>. Wrong parameter types or a non-assignable return type cause <see cref="FactoryDiscovery{TOwner,TPart}.RegisterAll"/> to throw at startup. Placing this attribute on a non-static method is silently skipped — only static methods are scanned.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class PrefabFactoryAttribute : Attribute
    {
        /// <summary> The explicit type-key to register under, or <c>null</c> to derive the key from the declaring type's name. </summary>
        /// <remarks> When <c>null</c>, <see cref="FactoryDiscovery{TOwner,TPart}"/> strips a configurable suffix from the declaring type name and normalises the remainder to snake_case. </remarks>
        public String? TypeKey { get; }


        /// <summary> Initialises the attribute, optionally pinning a specific type-key. </summary>
        /// <param name="typeKey"> The type-key to register under. Pass <c>null</c> or omit to derive the key from the declaring class name. </param>
        protected PrefabFactoryAttribute(String? typeKey = null)
        {
            TypeKey = typeKey;
        }
    }
}
