using System;
using System.Collections.Generic;

namespace Khepri.Prefabs
{
    /// <summary> A reusable, data-driven definition of a standard part set that can be applied to any number of fresh owner objects without modification. </summary>
    /// <typeparam name="TOwner"> The type of the object that owns the parts; must implement <see cref="IPartContainer{TPart}"/> for the builder to attach them. </typeparam>
    /// <typeparam name="TPart"> The base type of parts produced by this prefab's factories. </typeparam>
    public sealed class Prefab<TOwner, TPart> where TOwner : IPartContainer<TPart>
    {
        /// <summary> The ordered sequence of part factories to invoke when applying this prefab to a builder. </summary>
        private readonly List<Func<TOwner, TPart>> _factories;


        /// <summary> Initialises a new, empty prefab. </summary>
        private Prefab()
        {
            _factories = new List<Func<TOwner, TPart>>();
        }


        /// <summary> Creates and returns a new empty prefab, ready for part definitions to be added. </summary>
        /// <returns> A new <see cref="Prefab{TOwner,TPart}"/> instance with no parts registered. </returns>
        public static Prefab<TOwner, TPart> Define()
        {
            return new Prefab<TOwner, TPart>();
        }


        /// <summary> Records a part factory on this prefab so that every owner created from it receives the produced part, and returns this prefab for further chaining. </summary>
        /// <param name="factory"> A delegate that receives the owner under construction and returns a fully constructed part. Forwarded verbatim to <see cref="PrefabBuilder{TOwner,TPart}.WithPart(Func{TOwner,TPart})"/> — must not return <c>null</c>. </param>
        /// <returns> This prefab instance, enabling a fluent call chain. </returns>
        public Prefab<TOwner, TPart> WithPart(Func<TOwner, TPart> factory)
        {
            _factories.Add(factory);
            return this;
        }


        /// <summary> Applies every part factory registered on this prefab to the supplied builder, in registration order, and returns the builder for further chaining. </summary>
        /// <remarks> Intentionally <c>internal</c> — callers interact via a domain-specific factory method (such as <c>EntityFactory.CreateFrom</c>). </remarks>
        /// <param name="builder"> The builder to apply this prefab's part set to. </param>
        /// <returns> The same <paramref name="builder"/> instance after all factories have been applied, ready for additional per-instance parts or a final <see cref="PrefabBuilder{TOwner,TPart}.Build"/> call. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from <see cref="PrefabBuilder{TOwner,TPart}.WithPart(Func{TOwner,TPart})"/> when a factory returns <c>null</c> or a duplicate part type is detected. </exception>
        internal PrefabBuilder<TOwner, TPart> ApplyTo(PrefabBuilder<TOwner, TPart> builder)
        {
            PrefabBuilder<TOwner, TPart> result = builder;

            foreach (Func<TOwner, TPart> factory in _factories)
            {
                result = result.WithPart(factory);
            }

            return result;
        }
    }
}
