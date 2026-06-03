using System;

namespace Khepri.Prefabs
{
    /// <summary> Fluent builder for constructing an owner object and attaching parts in a single expression. </summary>
    /// <remarks> Obtain an instance via a domain-specific factory method — do not construct directly. </remarks>
    /// <typeparam name="TOwner"> The type of the object being assembled; must implement <see cref="IPartContainer{TPart}"/> so parts can be attached. </typeparam>
    /// <typeparam name="TPart"> The base type of parts accepted by <typeparamref name="TOwner"/>. </typeparam>
    public class PrefabBuilder<TOwner, TPart> where TOwner : IPartContainer<TPart>
    {
        /// <summary> The owner object being assembled by this builder. </summary>
        private readonly TOwner _owner;


        /// <summary> Initialises a new builder that will assemble the supplied owner object. </summary>
        /// <param name="owner"> The owner to attach parts to. </param>
        internal PrefabBuilder(TOwner owner)
        {
            _owner = owner;
        }


        /// <summary> Invokes <paramref name="factory"/> with the owner under construction, attaches the produced part, and returns this builder for further chaining. </summary>
        /// <typeparam name="T"> The concrete part type being added; used to name the type in error messages. </typeparam>
        /// <param name="factory"> A delegate that receives the owner and returns a fully constructed part. Must not return <c>null</c>. </param>
        /// <returns> This builder instance, enabling a fluent call chain. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="factory"/> returns <c>null</c>, or when a duplicate part type is already attached to the owner. </exception>
        public PrefabBuilder<TOwner, TPart> WithPart<T>(Func<TOwner, T> factory) where T : TPart
        {
            T part = factory(_owner);

            if (part is null)
            {
                throw new InvalidOperationException($"The factory delegate for part type '{typeof(T).Name}' returned null.");
            }

            return ApplyPart(part);
        }


        /// <summary> Invokes <paramref name="factory"/> with the owner under construction, attaches the produced part, and returns this builder for further chaining. </summary>
        /// <remarks> Non-generic overload for the prefab path; errors report the part's runtime type rather than a compile-time parameter. </remarks>
        /// <param name="factory"> A delegate that receives the owner and returns a fully constructed part. Must not return <c>null</c>. </param>
        /// <returns> This builder instance, enabling a fluent call chain. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <paramref name="factory"/> returns <c>null</c>, or when a duplicate part type is already attached to the owner. </exception>
        public PrefabBuilder<TOwner, TPart> WithPart(Func<TOwner, TPart> factory)
        {
            TPart part = factory(_owner);

            if (part is null)
            {
                throw new InvalidOperationException("The part factory delegate returned null.");
            }

            return ApplyPart(part);
        }


        /// <summary> Attaches <paramref name="part"/> to the owner under construction and returns this builder, throwing if the container rejects it as a duplicate. </summary>
        /// <param name="part"> The fully constructed, non-null part to attach. </param>
        /// <returns> This builder instance, enabling a fluent call chain. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <see cref="IPartContainer{TPart}.Add"/> returns <c>false</c>, indicating the part was already present. </exception>
        private PrefabBuilder<TOwner, TPart> ApplyPart(TPart part)
        {
            Boolean added = _owner.Add(part);

            if (!added)
            {
                throw new InvalidOperationException($"A part of type '{part!.GetType().Name}' is already attached to the owner.");
            }

            return this;
        }


        /// <summary> Returns the fully assembled owner object, completing the build. </summary>
        /// <returns> The owner with all parts attached. </returns>
        public TOwner Build()
        {
            return _owner;
        }
    }
}
