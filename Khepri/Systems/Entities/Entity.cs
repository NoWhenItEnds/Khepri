using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Khepri.Entities.Components;

namespace Khepri.Entities
{
    /// <summary> A thing or object that can exist within a room. All things, including players and items, are entities. </summary>
    public class Entity : IEquatable<Entity>
    {
        /// <summary> The entity's unique identifier. Should be unique across all entities. </summary>
        public readonly Guid UId;

        // TODO - This should have a dynamic description, like a room, and a generated texture...

        /// <summary> The set of all components currently attached to this entity. </summary>
        private readonly HashSet<Component> _components = new HashSet<Component>();


        /// <summary> Initialises a new instance of the <see cref="Entity"/> class. </summary>
        /// <param name="uid"> The unique identifier for the entity. </param>
        public Entity(Guid uid)
        {
            UId = uid;
        }


        /// <summary> Adds a pre-constructed component instance to this entity. </summary>
        /// <param name="component"> The component being added. </param>
        /// <returns> <c>true</c> if the component was added; <c>false</c> if an equal component already exists. </returns>
        public Boolean AddComponent(Component component) => _components.Add(component);


        /// <summary> Gets the first attached component of type <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The kind of component to retrieve. </typeparam>
        /// <returns> The component instance, or <c>null</c> if none is attached. </returns>
        public T? GetComponent<T>() where T : Component => _components.OfType<T>().FirstOrDefault();


        /// <summary> Removes all attached components whose runtime type is exactly <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The type of component to remove. </typeparam>
        /// <returns> <c>true</c> if at least one component was removed; <c>false</c> if none were found. </returns>
        public Boolean RemoveComponent<T>() where T : Component
        {
            List<Component> matches = _components.Where(x => x.GetType().Equals(typeof(T))).ToList();

            foreach (Component match in matches)
            {
                _components.Remove(match);
            }

            return matches.Count > 0;
        }


        /// <summary> Removes a specific component instance from this entity. </summary>
        /// <param name="component"> The component to remove. </param>
        /// <returns> <c>true</c> if the component was removed; <c>false</c> if it was not attached. </returns>
        public Boolean RemoveComponent(Component component) => _components.Remove(component);


        /// <summary> Build a dynamic description of the entity's current state to display to the player. </summary>
        /// <returns> A richly formatted string to display to the player representing the entity's current state / situation. </returns>
        public String BuildDescription()
        {
            return "This is an entity.";   // TODO - Figure out how to do this!
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
