using System;
using Khepri.Prefabs;
using Khepri.Rooms.Features;

namespace Khepri.Rooms.Prefabs
{
    /// <summary> Scans the <c>Khepri</c> assembly for static methods decorated with <see cref="FeatureFactoryAttribute"/> and registers each as a room feature factory in a <see cref="FeatureRegistry"/>. </summary>
    /// <remarks> Call <see cref="RegisterAll"/> once at the composition root, before any prefab loading begins. </remarks>
    public static class FeatureDiscovery
    {
        /// <summary> The suffix stripped from a declaring type name when the attribute's <see cref="FeatureFactoryAttribute.TypeKey"/> is <c>null</c>. </summary>
        private const String FeatureSuffix = "Feature";


        /// <summary> Scans the <c>Khepri</c> assembly, validates every method decorated with <see cref="FeatureFactoryAttribute"/>, and registers each as a factory in <paramref name="registry"/>. </summary>
        /// <remarks> All violations are collected before throwing so the full list of offending methods appears in a single error message rather than requiring repeated fix-and-retry cycles. </remarks>
        /// <param name="registry"> The registry to populate; each valid factory is registered exactly once. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when one or more decorated methods violate the factory contract (wrong parameter types, or return type not assignable to <see cref="Feature"/>).
        /// The message names every offending method and its declaring type.
        /// Also propagated from <see cref="FeatureRegistry.Register"/> when two methods resolve to the same type-key.
        /// Note: a <see cref="FeatureFactoryAttribute"/> placed on a non-static method is silently skipped — only static methods are scanned.
        /// </exception>
        public static void RegisterAll(FeatureRegistry registry)
        {
            FactoryDiscovery<Room, Feature> discovery = new FactoryDiscovery<Room, Feature>(
                typeof(FeatureFactoryAttribute),
                FeatureSuffix,
                typeof(Feature).Assembly);

            discovery.RegisterAll(registry);
        }
    }
}
