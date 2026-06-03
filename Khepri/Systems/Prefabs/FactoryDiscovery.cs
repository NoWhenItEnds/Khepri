using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Khepri.Prefabs
{
    /// <summary> Scans a given assembly for static methods decorated with a specific <see cref="PrefabFactoryAttribute"/> subtype and registers each as a part factory in a <see cref="FactoryRegistry{TOwner,TPart}"/>. </summary>
    /// <remarks> Call <see cref="RegisterAll"/> once at the composition root, before any prefab loading begins. </remarks>
    /// <typeparam name="TOwner"> The owning object type that all registered factories accept as their first parameter. </typeparam>
    /// <typeparam name="TPart"> The base part type that all registered factories must return. </typeparam>
    public sealed class FactoryDiscovery<TOwner, TPart>
    {
        /// <summary> The exact attribute type to scan for on static methods. </summary>
        private readonly Type _attributeType;

        /// <summary> The suffix stripped from a declaring type name when the attribute's <see cref="PrefabFactoryAttribute.TypeKey"/> is <c>null</c>. </summary>
        private readonly String _typeSuffix;

        /// <summary> The assembly to scan for decorated factory methods. </summary>
        private readonly Assembly _assembly;


        /// <summary> Initialises the discovery helper with its scan parameters. </summary>
        /// <param name="attributeType"> The <see cref="PrefabFactoryAttribute"/> subtype to scan for. Must be <see cref="PrefabFactoryAttribute"/> or a subclass. </param>
        /// <param name="typeSuffix"> The suffix stripped from the declaring type name when deriving a key (e.g. <c>"Component"</c>). Pass an empty string to use the full type name unchanged. </param>
        /// <param name="assembly"> The assembly to scan. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="attributeType"/> is not assignable to <see cref="PrefabFactoryAttribute"/>. </exception>
        public FactoryDiscovery(Type attributeType, String typeSuffix, Assembly assembly)
        {
            Boolean isValidAttributeType = typeof(PrefabFactoryAttribute).IsAssignableFrom(attributeType);

            if (!isValidAttributeType)
            {
                throw new ArgumentException($"'{attributeType.FullName}' must be assignable to PrefabFactoryAttribute.", nameof(attributeType));
            }

            _attributeType = attributeType;
            _typeSuffix    = typeSuffix ?? String.Empty;
            _assembly      = assembly;
        }


        /// <summary> Scans the configured assembly, validates every method decorated with the configured attribute type, and registers each as a factory in <paramref name="registry"/>. </summary>
        /// <remarks> All violations are collected before throwing so the full list of offending methods appears in a single error message rather than requiring repeated fix-and-retry cycles. </remarks>
        /// <param name="registry"> The registry to populate; each valid factory is registered exactly once. </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when one or more decorated methods violate the factory contract (wrong parameter types, or return type not assignable to <typeparamref name="TPart"/>).
        /// The message names every offending method and its declaring type.
        /// Also propagated from <see cref="FactoryRegistry{TOwner,TPart}.Register"/> when two methods resolve to the same type-key.
        /// Note: a factory attribute placed on a non-static method is silently skipped — only static methods are scanned.
        /// </exception>
        public void RegisterAll(FactoryRegistry<TOwner, TPart> registry)
        {
            RegisterAll(registry, typeKeyMap: null);
        }


        /// <summary> Scans the configured assembly, validates every method decorated with the configured attribute type, registers each as a factory in <paramref name="registry"/>, and optionally records every declaring-type-to-key mapping in <paramref name="typeKeyMap"/>. </summary>
        /// <remarks>
        /// When <paramref name="typeKeyMap"/> is supplied, each factory's declaring type is mapped to its resolved key, making this the single assembly scan that populates both the spawn registry and the reverse serialisation map.
        /// All violations are collected before throwing so the full list of offending methods appears in a single error message rather than requiring repeated fix-and-retry cycles.
        /// </remarks>
        /// <param name="registry"> The registry to populate; each valid factory is registered exactly once. </param>
        /// <param name="typeKeyMap"> When non-null, receives a declaring-type-to-key entry for each registered factory, enabling the serialiser to look up the type-key for a live part instance. Pass <c>null</c> when no reverse lookup is needed (e.g. for room features). </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when one or more decorated methods violate the factory contract.
        /// Also propagated from <see cref="FactoryRegistry{TOwner,TPart}.Register"/> or <see cref="TypeKeyMap.Register"/> when two methods resolve to the same type-key or the same declaring type.
        /// Note: a factory attribute placed on a non-static method is silently skipped — only static methods are scanned.
        /// </exception>
        public void RegisterAll(FactoryRegistry<TOwner, TPart> registry, TypeKeyMap? typeKeyMap)
        {
            IEnumerable<MethodInfo> candidates = FindCandidates();

            List<String>                                                     violations    = new List<String>();
            List<(String key, Type declaringType, Func<TOwner, PrefabData, TPart> factory)> registrations
                = ProcessCandidatesWithTypes(candidates, violations);

            Boolean hasViolations = violations.Count > 0;

            if (hasViolations)
            {
                throw new InvalidOperationException(BuildViolationMessage(violations));
            }

            foreach ((String key, Type declaringType, Func<TOwner, PrefabData, TPart> factory) in registrations)
            {
                registry.Register(key, factory);

                if (typeKeyMap is not null)
                {
                    typeKeyMap.Register(declaringType, key);
                }
            }
        }


        /// <summary> Validates each candidate, collecting valid factory registrations (with declaring types) into the returned list and appending a violation message for every invalid candidate to <paramref name="violations"/>. </summary>
        /// <param name="candidates"> The decorated methods to validate. </param>
        /// <param name="violations"> Accumulator to which one message is appended per invalid candidate; mutated as a side effect. </param>
        /// <returns> The resolved key, declaring type, and bound factory delegate for every candidate that passed validation; may be empty. </returns>
        private List<(String key, Type declaringType, Func<TOwner, PrefabData, TPart> factory)> ProcessCandidatesWithTypes(
            IEnumerable<MethodInfo> candidates,
            List<String>            violations)
        {
            List<(String key, Type declaringType, Func<TOwner, PrefabData, TPart> factory)> registrations
                = new List<(String, Type, Func<TOwner, PrefabData, TPart>)>();

            foreach (MethodInfo method in candidates)
            {
                String? violation = ValidateCandidate(method);

                if (violation is not null)
                {
                    violations.Add(violation);
                }
                else
                {
                    String                          key          = ResolveKey(method);
                    Type                            declaringType = method.DeclaringType!;
                    Func<TOwner, PrefabData, TPart> factory      = BindFactory(method);
                    registrations.Add((key, declaringType, factory));
                }
            }

            return registrations;
        }


        /// <summary> Assembles the consolidated error message that lists every collected violation as a bullet point. </summary>
        /// <param name="violations"> The per-method violation descriptions to enumerate; expected to be non-empty. </param>
        /// <returns> A multi-line message naming every offending method. </returns>
        private String BuildViolationMessage(IReadOnlyList<String> violations)
        {
            StringBuilder message = new StringBuilder(
                $"FactoryDiscovery found one or more invalid factory methods. " +
                $"Each method decorated with [{_attributeType.Name}] must accept ({typeof(TOwner).Name}, PrefabData) " +
                $"and return a type assignable to {typeof(TPart).Name}. (Non-static methods are not scanned.)");

            foreach (String violation in violations)
            {
                message.AppendLine();
                message.Append("  - ");
                message.Append(violation);
            }

            return message.ToString();
        }


        /// <summary> Enumerates all static methods in the configured assembly that carry the configured attribute type, searching all types and all access levels. </summary>
        /// <remarks> Only static methods are scanned; a factory attribute placed on a non-static method is silently skipped. The team has accepted this trade-off. </remarks>
        /// <returns> A sequence of static methods that bear the attribute; may be empty. </returns>
        private IEnumerable<MethodInfo> FindCandidates()
        {
            List<MethodInfo> candidates = new List<MethodInfo>();

            foreach (Type type in _assembly.GetTypes())
            {
                MethodInfo[] methods = type.GetMethods(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                foreach (MethodInfo method in methods)
                {
                    Boolean hasAttribute = method.IsDefined(_attributeType, inherit: false);

                    if (hasAttribute)
                    {
                        candidates.Add(method);
                    }
                }
            }

            return candidates;
        }


        /// <summary> Checks that <paramref name="method"/> satisfies every factory contract requirement and returns a consolidated violation message, or <c>null</c> when the method is valid. </summary>
        /// <param name="method"> The candidate method to validate. </param>
        /// <returns> A non-null description listing every contract violation for the method, or <c>null</c> when the method is valid. </returns>
        private String? ValidateCandidate(MethodInfo method)
        {
            String       location = $"{method.DeclaringType?.FullName ?? "<unknown>"}.{method.Name}";
            List<String> defects  = new List<String>();

            Boolean returnsPart = typeof(TPart).IsAssignableFrom(method.ReturnType);

            ParameterInfo[] parameters        = method.GetParameters();
            Boolean         correctArity      = parameters.Length == 2;
            Boolean         firstParamIsOwner = correctArity && parameters[0].ParameterType == typeof(TOwner);
            Boolean         secondParamIsData = correctArity && parameters[1].ParameterType == typeof(PrefabData);
            Boolean         signatureValid    = firstParamIsOwner && secondParamIsData;

            // The derived key only resolves meaningfully once the method has a declaring type;
            // an empty result means the type name strips to nothing (e.g. a type named exactly equal to the suffix).
            Boolean keyEmpty = ResolveKey(method).Length == 0;

            if (!returnsPart)
            {
                defects.Add($"return type '{method.ReturnType.Name}' is not assignable to {typeof(TPart).Name}");
            }

            if (!signatureValid)
            {
                defects.Add($"parameters must be exactly ({typeof(TOwner).Name}, PrefabData)");
            }

            if (keyEmpty)
            {
                defects.Add($"derives an empty type-key; specify an explicit key via [{_attributeType.Name}] or rename the declaring type");
            }

            Boolean isValid = defects.Count == 0;

            String? result = isValid
                ? null
                : $"{location}: {String.Join("; ", defects)}.";

            return result;
        }


        /// <summary> Determines the type-key to register for <paramref name="method"/>: uses the attribute's explicit key when set, otherwise strips the configured suffix from the declaring type name. </summary>
        /// <param name="method"> A method bearing the configured factory attribute. </param>
        /// <returns> The raw (not yet snake_case-normalised) type-key string; normalisation is performed by <see cref="FactoryRegistry{TOwner,TPart}.Register"/>. </returns>
        private String ResolveKey(MethodInfo method)
        {
            PrefabFactoryAttribute attribute    = (PrefabFactoryAttribute)method.GetCustomAttribute(_attributeType)!;
            String                 declaringName = method.DeclaringType!.Name;

            Boolean hasExplicitKey = attribute.TypeKey is not null;

            String rawKey = hasExplicitKey
                ? attribute.TypeKey!
                : StripSuffix(declaringName);

            return rawKey;
        }


        /// <summary> Strips the configured suffix from <paramref name="typeName"/> if present, otherwise returns the name unchanged. </summary>
        /// <param name="typeName"> The declaring type's simple name. </param>
        /// <returns> The name without the suffix, or the original name when the suffix is absent or empty. </returns>
        private String StripSuffix(String typeName)
        {
            Boolean hasSuffix = _typeSuffix.Length > 0 && typeName.EndsWith(_typeSuffix, StringComparison.Ordinal);

            String result = hasSuffix
                ? typeName.Substring(0, typeName.Length - _typeSuffix.Length)
                : typeName;

            return result;
        }


        /// <summary> Creates a <see cref="Func{TOwner, PrefabData, TPart}"/> delegate bound to <paramref name="method"/> using return-type covariance. </summary>
        /// <param name="method"> A validated static factory method whose return type is assignable to <typeparamref name="TPart"/>. </param>
        /// <returns> A delegate that invokes the factory method. </returns>
        private static Func<TOwner, PrefabData, TPart> BindFactory(MethodInfo method)
        {
            return method.CreateDelegate<Func<TOwner, PrefabData, TPart>>();
        }
    }
}
