using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.ECS;
using Khepri.Descriptions;
using Khepri.Entities.Components.Parts;

namespace Khepri.Entities.Components
{
    /// <summary> The single owner of an entity's gestalt identity — holds the entity's parts and delegates naming entirely to them. </summary>
    /// <remarks>
    /// Implements <see cref="IMultiComponentHolder{T}"/> directly over the exported <see cref="Parts"/> array, which stays the
    /// single source of truth — it is what the Inspector authors and what Godot serialises. Multiplicity is supported: an entity
    /// may have four wheels and two arms without violating the one-component-per-type rule on the entity itself.
    /// </remarks>
    [GlobalClass]
    public partial class IdentityComponent : Component, IDescriptionContributor, IMultiComponentHolder<PartComponent>
    {
        /// <summary> The entity's anatomical or structural parts, authored in the Inspector. Backs every <see cref="IMultiComponentHolder{T}"/> member on this component. </summary>
        [Export] public Godot.Collections.Array<PartComponent> Parts { get; set; } = new Godot.Collections.Array<PartComponent>();


        /// <inheritdoc/>
        public event Action<PartComponent>? ComponentAdded;


        /// <inheritdoc/>
        public event Action<PartComponent>? ComponentRemoved;


        /// <summary> Adds a part instance to this identity. </summary>
        /// <param name="component"> The part to attach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the part was added; <c>false</c> if that exact instance is already attached. Distinct instances of the same concrete type are always accepted. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean AddComponent(PartComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            Boolean alreadyAttached = Parts.Any(part => ReferenceEquals(part, component));
            if (!alreadyAttached)
            {
                Parts.Add(component);
                ComponentAdded?.Invoke(component);
            }
            return !alreadyAttached;
        }


        /// <summary> Returns all parts currently attached to this identity, in authored order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect <see cref="Parts"/>. </remarks>
        /// <returns> A read-only snapshot of every attached part. </returns>
        public IReadOnlyCollection<PartComponent> GetComponents() => new List<PartComponent>(Parts);


        /// <summary> Returns all attached parts whose concrete runtime type is assignable to <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The part type to retrieve, including anything derived from it. </typeparam>
        /// <returns> A read-only snapshot of every matching attached part; empty if none are attached. </returns>
        public IReadOnlyCollection<TComponent> GetComponents<TComponent>() where TComponent : PartComponent
            => Parts.OfType<TComponent>().ToList();


        /// <summary> Checks whether at least one part assignable to <typeparamref name="TComponent"/> is currently attached. </summary>
        /// <typeparam name="TComponent"> The part type to test for, including anything derived from it. </typeparam>
        /// <returns> <c>true</c> if at least one matching part is attached; <c>false</c> otherwise. </returns>
        public Boolean HasComponent<TComponent>() where TComponent : PartComponent => Parts.OfType<TComponent>().Any();


        /// <summary> Returns the number of attached parts whose concrete runtime type is assignable to <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The part type to count, including anything derived from it. </typeparam>
        /// <returns> The count of matching attached parts; zero if none are attached. </returns>
        public Int32 CountComponents<TComponent>() where TComponent : PartComponent => Parts.OfType<TComponent>().Count();


        /// <summary> Removes a specific part instance from this identity. </summary>
        /// <param name="component"> The exact part instance to detach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the part was removed; <c>false</c> if it was not attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        public Boolean RemoveComponent(PartComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            Boolean removed = Parts.Remove(component);
            if (removed)
            {
                ComponentRemoved?.Invoke(component);
            }
            return removed;
        }


        /// <summary> Removes all attached parts whose concrete runtime type is assignable to <typeparamref name="TComponent"/>. </summary>
        /// <remarks> <see cref="ComponentRemoved"/> is raised once per removed part. </remarks>
        /// <typeparam name="TComponent"> The part type to remove, including anything derived from it. </typeparam>
        /// <returns> <c>true</c> if at least one part was removed; <c>false</c> if none of that type were attached. </returns>
        public Boolean RemoveComponents<TComponent>() where TComponent : PartComponent
        {
            List<TComponent> matches = Parts.OfType<TComponent>().ToList();
            foreach (TComponent match in matches)
            {
                Parts.Remove(match);
                ComponentRemoved?.Invoke(match);
            }
            return matches.Count > 0;
        }


        /// <inheritdoc/>
        public override void Validate(EntityPrefab prefab)
        {
            if (Parts.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Entity prefab '{prefab.Name}' has an IdentityComponent with no parts. Every entity must have at least one part so it always resolves to a name.");
            }

            foreach (PartComponent part in Parts)
            {
                part.Validate(prefab);
            }
        }


        /// <summary> Resolves the entity's noun by letting each part stake its claim. </summary>
        /// <returns> The resolved noun, or a fallback label when no part claims one. </returns>
        public String GetNoun()
        {
            // Take the most popular claim as the winner.
            Dictionary<String, Int32> claims = new Dictionary<String, Int32>(); // TODO - Kind should use a map or something. Something slightly more complected.
            Parts.GroupBy(part => part.Kind.Noun).ToList().ForEach(group =>
            {
                String claim = group.Key;
                Int32 count = group.Count();
                claims[claim] = count;
            });

            return claims.Keys.Count > 0 ? claims.OrderBy(x => x.Value).Last().Key : "something";
        }


        /// <inheritdoc/>
        public void Contribute(DescriptionBuilder builder)
        {
            foreach (PartComponent part in Parts)
            {
                part.Contribute(builder);
            }
        }
    }
}
