using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.Extensions;
using Khepri.Descriptions;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> A thing or object that can exist within a room. All things, including players and items, are entities. </summary>
    public class Entity : IEquatable<Entity>, INoteSource
    {
        /// <summary> The entity's unique identifier. Should be unique across all entities. </summary>
        public readonly Guid UId;


        /// <summary> All components currently attached to this entity, keyed by their exact runtime type — an entity holds at most one component of each concrete type. </summary>
        /// <remarks> Keying by <see cref="Type"/> rather than relying on component equality keeps the uniqueness rule explicit and avoids overriding equality on the Godot <c>Resource</c> base. </remarks>
        private readonly Dictionary<Type, Component> _components = new Dictionary<Type, Component>();


        /// <summary> Initialises a new instance of the <see cref="Entity"/> class. </summary>
        /// <param name="uid"> The unique identifier for the entity. </param>
        public Entity(Guid uid)
        {
            UId = uid;
        }


        /// <summary> Adds a component instance to this entity. </summary>
        /// <param name="component"> The component being added. </param>
        /// <returns> <c>true</c> if the component was added; <c>false</c> if a component of the same concrete type is already attached. </returns>
        public Boolean AddComponent(Component component) => _components.TryAdd(component.GetType(), component);


        /// <summary> Gets the first attached component assignable to <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The kind of component to retrieve. </typeparam>
        /// <returns> The component instance, or <c>null</c> if none is attached. </returns>
        public T? GetComponent<T>() where T : Component => _components.Values.OfType<T>().FirstOrDefault();


        /// <summary> Returns all components currently attached to this entity, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the entity's internal component set. </remarks>
        /// <returns> A read-only snapshot of every attached component. </returns>
        public IReadOnlyCollection<Component> GetComponents() => _components.Values.ToList();


        /// <summary> Returns only the components that implement <see cref="IEntityContainer"/>, allowing callers to walk the containment hierarchy without exposing the full component set. </summary>
        /// <returns> The subset of attached components that act as entity containers, in unspecified order. </returns>
        public IReadOnlyCollection<IEntityContainer> GetContainers() => _components.Values.OfType<IEntityContainer>().ToList();


        /// <summary> Removes the attached component whose runtime type is exactly <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The type of component to remove. </typeparam>
        /// <returns> <c>true</c> if a component was removed; <c>false</c> if none was found. </returns>
        public Boolean RemoveComponent<T>() where T : Component => _components.Remove(typeof(T));


        /// <summary> Removes a specific component instance from this entity. </summary>
        /// <param name="component"> The component to remove. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if it was not attached. </returns>
        public Boolean RemoveComponent(Component component)
        {
            Boolean present = _components.TryGetValue(component.GetType(), out Component? attached) && ReferenceEquals(attached, component);
            return present && _components.Remove(component.GetType());
        }


        /// <summary> Builds a dynamic description of the entity's current state — its tooltip body when it appears as a note. </summary>
        /// <returns> The assembled description of the entity's current state. </returns>
        public Description BuildDescription()
        {
            DescriptionBuilder builder = new DescriptionBuilder();

            // Open by naming what the entity is, then let its components add detail.
            builder.Text(GetName().ToCapitalised() + ".");

            List<Action<DescriptionBuilder>> contributions = new List<Action<DescriptionBuilder>>();

            foreach (Component component in _components.Values)
            {
                contributions.Add(component.Contribute);
            }

            Description detail = DescriptionBuilder.Compose(contributions);
            if (detail.Spans.Count > 0)
            {
                builder.Text(" ").Append(detail);
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
            NameBuilder builder = new NameBuilder();

            foreach (Component component in _components.Values)
            {
                component.Contribute(builder);
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
