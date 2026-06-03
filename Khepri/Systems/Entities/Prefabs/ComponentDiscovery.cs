using System;
using Khepri.Entities.Components;
using Khepri.Prefabs;

namespace Khepri.Entities.Prefabs
{
    /// <summary> Scans the <c>Khepri</c> assembly for static methods decorated with <see cref="ComponentFactoryAttribute"/> and registers each as a component factory in a <see cref="ComponentRegistry"/>, optionally also populating a <see cref="TypeKeyMap"/> for serialisation. </summary>
    /// <remarks>
    /// Call <see cref="RegisterAll(ComponentRegistry, TypeKeyMap)"/> once at the composition root before any prefab loading or serialisation begins.
    /// This is the single assembly scan that covers both the spawn/restore factory registry and the reverse type-to-key map used by the serialiser.
    /// </remarks>
    public static class ComponentDiscovery
    {
        /// <summary> The suffix stripped from a declaring type name when the attribute's <see cref="ComponentFactoryAttribute.TypeKey"/> is <c>null</c>. </summary>
        private const String ComponentSuffix = "Component";


        /// <summary> Scans the <c>Khepri</c> assembly, validates every method decorated with <see cref="ComponentFactoryAttribute"/>, registers each factory in <paramref name="registry"/>, and records each declaring-type-to-key mapping in <paramref name="typeKeyMap"/>. </summary>
        /// <remarks>
        /// All violations are collected before throwing so the full list of offending methods appears in a single error message rather than requiring repeated fix-and-retry cycles.
        /// The <paramref name="typeKeyMap"/> is populated from the same scan so no second pass over the assembly is needed.
        /// </remarks>
        /// <param name="registry"> The registry to populate; each valid factory is registered exactly once. </param>
        /// <param name="typeKeyMap"> The map to populate with component-type-to-key entries; used by <see cref="EntitySerialiser"/> to write the <c>"type"</c> field during serialisation. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when one or more decorated methods violate the factory contract (wrong parameter types, or return type not assignable to <see cref="Component"/>).
        /// The message names every offending method and its declaring type.
        /// Also propagated from <see cref="ComponentRegistry.Register"/> when two methods resolve to the same type-key.
        /// Note: a <see cref="ComponentFactoryAttribute"/> placed on a non-static method is silently skipped — only static methods are scanned.
        /// </exception>
        public static void RegisterAll(ComponentRegistry registry, TypeKeyMap typeKeyMap)
        {
            FactoryDiscovery<Entity, Component> discovery = new FactoryDiscovery<Entity, Component>(
                typeof(ComponentFactoryAttribute),
                ComponentSuffix,
                typeof(Component).Assembly);

            discovery.RegisterAll(registry, typeKeyMap);
        }
    }
}
