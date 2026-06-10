using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.ECS;
using Jaypen.Utilities.Extensions;
using Khepri.Descriptions;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> A thing or object that can exist within a room. All things, including players and items, are entities. </summary>
    public class Entity : IEquatable<Entity>, INoteSource, ISingleComponentHolder<Component>
    {
        /// <summary> The entity's unique identifier. Should be unique across all entities. </summary>
        public readonly Guid UId;


        /// <summary> Delegate storage for all components attached to this entity — at most one per exact concrete type. </summary>
        private readonly ComponentStore<Component> _components = new ComponentStore<Component>();


        /// <summary> Initialises a new instance of the <see cref="Entity"/> class. </summary>
        /// <param name="uid"> The unique identifier for the entity. </param>
        public Entity(Guid uid)
        {
            UId = uid;
        }


        /// <summary> Raised after a component is successfully attached to this entity, passing the newly added component as the argument. </summary>
        /// <remarks> Subscribers are invoked synchronously after the internal collection has already been mutated — the entity's component set already reflects the change when handlers run. </remarks>
        public event Action<Component>? ComponentAdded;


        /// <summary> Raised after a component is successfully detached from this entity, passing the removed component as the argument. </summary>
        /// <remarks> Subscribers are invoked synchronously after the internal collection has already been mutated — the entity's component set already reflects the change when handlers run. </remarks>
        public event Action<Component>? ComponentRemoved;


        /// <summary> Adds a component instance to this entity. </summary>
        /// <param name="component"> The component to attach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was added; <c>false</c> if a component of the same concrete type is already attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="Component.Initialise"/> (which sets <c>Owner</c>) before raising
        /// <see cref="ComponentAdded"/>, so observers always see a fully initialised component.
        /// </remarks>
        public Boolean AddComponent(Component component)
        {
            Boolean added = _components.Add(component);
            if (added) { component.Initialise(this); ComponentAdded?.Invoke(component); }
            return added;
        }


        /// <summary> Returns all components currently attached to this entity, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the entity's internal component set. </remarks>
        /// <returns> A read-only snapshot of every attached component. </returns>
        public IReadOnlyCollection<Component> GetComponents() => _components.GetAll();


        /// <summary> Checks whether a component whose concrete runtime type is exactly <typeparamref name="TComponent"/> is currently attached. </summary>
        /// <typeparam name="TComponent"> The exact concrete component type to test for. </typeparam>
        /// <returns> <c>true</c> if a component of that exact type is attached; <c>false</c> otherwise. </returns>
        public Boolean HasComponent<TComponent>() where TComponent : Component => _components.Has<TComponent>();


        /// <summary> Attempts to retrieve the attached component whose concrete runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <remarks>
        /// Uses an exact-type dictionary lookup, not assignability scanning. A subclass of <typeparamref name="TComponent"/> stored under its
        /// own key will not be found here.
        /// Because <typeparamref name="TComponent"/> is constrained to a reference type in this implementation, <c>default</c> on a miss is
        /// <c>null</c>; callers should still use the Boolean return value as the authoritative presence check.
        /// </remarks>
        /// <typeparam name="TComponent"> The exact concrete component type to retrieve. </typeparam>
        /// <param name="component"> Contains the attached component when this method returns <c>true</c>; otherwise <c>default(<typeparamref name="TComponent"/>)</c>. </param>
        /// <returns> <c>true</c> if the matching component was found; <c>false</c> if none is attached. </returns>
        public Boolean TryGetComponent<TComponent>(out TComponent component) where TComponent : Component
        {
            return _components.TryGet<TComponent>(out component);
        }


        /// <summary> Returns only the components that implement <see cref="IEntityContainer"/>, allowing callers to walk the containment hierarchy without exposing the full component set. </summary>
        /// <returns> The subset of attached components that act as entity containers, in unspecified order. </returns>
        public IReadOnlyCollection<IEntityContainer> GetContainers() => _components.GetAll().OfType<IEntityContainer>().ToList();


        /// <summary> Removes the attached component whose runtime type is exactly <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The type of component to remove. </typeparam>
        /// <returns> <c>true</c> if a component was removed; <c>false</c> if none was found. </returns>
        /// <remarks>
        /// On success: calls <see cref="Component.Detach"/> (which clears <c>Owner</c>) before raising
        /// <see cref="ComponentRemoved"/>, so <c>Owner</c> is already <c>null</c> when observers see the removal.
        /// </remarks>
        public Boolean RemoveComponent<T>() where T : Component
        {
            Boolean removed = _components.Remove<T>(out Component? removedComponent);
            if (removed) { removedComponent!.Detach(); ComponentRemoved?.Invoke(removedComponent); }
            return removed;
        }


        /// <summary> Removes a specific component instance from this entity. </summary>
        /// <param name="component"> The exact component instance to detach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if it was not attached or a different instance of the same type is attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="Component.Detach"/> (which clears <c>Owner</c>) before raising
        /// <see cref="ComponentRemoved"/>, so <c>Owner</c> is already <c>null</c> when observers see the removal.
        /// </remarks>
        public Boolean RemoveComponent(Component component)
        {
            Boolean removed = _components.Remove(component);
            if (removed) { component.Detach(); ComponentRemoved?.Invoke(component); }
            return removed;
        }


        /// <summary> Builds a dynamic description of the entity's current state — its tooltip body when it appears as a note. </summary>
        /// <returns> The assembled description of the entity's current state. </returns>
        public Description BuildDescription()
        {
            DescriptionBuilder builder = new DescriptionBuilder();

            // Open by naming what the entity is, then let its components add detail, each fold separated from the last.
            builder.Text(GetName().ToCapitalised() + ".");

            foreach (IDescriptionContributor contributor in _components.GetAll().OfType<IDescriptionContributor>())
            {
                contributor.Contribute(builder);
            }

            return builder.Build();
        }


        /// <summary> Appends this entity to its container's description as a single hoverable note, labelled with its current name and pointing back at itself. </summary>
        /// <remarks> Called when the entity is listed inside something else — a room, or a container component such as an inventory. Hovering the note surfaces this entity's own <see cref="BuildDescription"/>. </remarks>
        /// <param name="builder"> The builder assembling the containing room's or entity's description. </param>
        public void Contribute(DescriptionBuilder builder)
        {
            builder.Note(GetName(), this);
        }


        /// <summary> Resolves the entity's current display name from its components. </summary>
        /// <remarks> The name emerges from whatever components are present — the most salient noun, decorated by the adjectives others contribute — so it tracks the entity through transformations rather than being fixed at spawn. </remarks>
        /// <returns> The composed name, or a fallback label when no component claims a noun. </returns>
        public String GetName()
        {
            // The noun comes solely from the identity component; absent one, the builder falls back.
            String noun = "something";
            if (TryGetComponent<IdentityComponent>(out IdentityComponent? identityComponent))
            {
                noun = identityComponent.GetNoun();
            }

            NameBuilder builder = NameBuilder.Create(noun);

            foreach (AdjectiveComponent adjective in _components.GetAll().OfType<AdjectiveComponent>())
            {
                builder.WithAdjective(adjective.RoyalIndex,adjective.GetAdjective());
            }

            return builder.Build();
        }


        /// <summary> Attempt to get a texture representing the current entity. </summary>
        /// <returns> A texture that represents the current entity's state / situation. A null indicates that there isn't one. </returns>
        public Texture2D? GetTexture()
        {
            return null;                // TODO - Figure out how to do this!
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => UId.GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Entity other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Entity? other) => other is not null && UId.Equals(other.UId);
    }
}
