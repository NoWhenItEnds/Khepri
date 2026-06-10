using Jaypen.Utilities.ECS;
using Khepri.Descriptions;
using Khepri.Entities;
using Khepri.Rooms.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Rooms
{
    /// <summary> A single self-contained playable space within the game world. It has entities within it, and connects to other rooms. </summary>
    public class Room : IEquatable<Room>, IEntityContainer, ISingleComponentHolder<Feature>
    {
        /// <summary> The room's unique identifier. Should be unique across all rooms. </summary>
        public readonly Guid UId;

        /// <summary> The rough size of the room in metres. </summary>
        /// <remarks> Used to calculate distances and interactions within the room. </remarks>
        public readonly Single Radius = 1f;


        /// <summary> The connections branching off from this current room. </summary>
        private readonly HashSet<Connection> _connections = new HashSet<Connection>();

        /// <summary> All the relative locations in the room and the entities occupying them. </summary>
        private readonly Dictionary<RoomPosition, List<Entity>> _entities = new Dictionary<RoomPosition, List<Entity>>();

        /// <summary> Delegate storage for all features attached to this room — at most one per exact concrete type. Features are aspects of the room itself; entities are its contents — they are separate collections. </summary>
        private readonly ComponentStore<Feature> _features = new ComponentStore<Feature>();


        /// <summary> Initialises a new instance of the <see cref="Room"/> class. </summary>
        /// <param name="uid"> The unique identifier for the room. </param>
        public Room(Guid uid)
        {
            UId = uid;

            // Initialise the entity lists for each position in the room.
            foreach (RoomPosition position in Enum.GetValues<RoomPosition>())
            {
                _entities[position] = new List<Entity>();
            }
        }


        /// <summary> Builds a dynamic description of the room's current state to display to the player. </summary>
        /// <remarks>
        /// The room's features contribute their prose in <see cref="Feature.Order"/>, then its entities each fold in as a hoverable note.
        /// Noteworthy spans in the result carry a reference to the feature or entity they describe, so the UI can show a tooltip sourced from that live object on hover.
        /// </remarks>
        /// <returns> The assembled description of the room's current state. </returns>
        public Description BuildDescription()
        {
            DescriptionBuilder builder = new DescriptionBuilder();

            foreach (Feature feature in _features.GetAll().OrderBy(feature => feature.Order))
            {
                feature.Contribute(builder);
            }

            foreach (Entity entity in GetEntities())
            {
                entity.Contribute(builder);
            }

            return builder.IsEmpty
                ? new DescriptionBuilder().Text("This is a room.").Build()
                : builder.Build();
        }


        /// <summary> Raised after a feature is successfully attached to this room, passing the newly added feature as the argument. </summary>
        /// <remarks> Subscribers are invoked synchronously after the internal collection has already been mutated — the room's feature set already reflects the change when handlers run. </remarks>
        public event Action<Feature>? ComponentAdded;


        /// <summary> Raised after a feature is successfully detached from this room, passing the removed feature as the argument. </summary>
        /// <remarks> Subscribers are invoked synchronously after the internal collection has already been mutated — the room's feature set already reflects the change when handlers run. </remarks>
        public event Action<Feature>? ComponentRemoved;


        /// <summary> Adds a feature instance to this room. </summary>
        /// <param name="feature"> The feature to attach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the feature was added; <c>false</c> if a feature of the same concrete type is already attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="feature"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="Feature.Initialise"/> (which sets <c>Owner</c>) before raising
        /// <see cref="ComponentAdded"/>, so observers always see a fully initialised feature.
        /// </remarks>
        public Boolean AddComponent(Feature feature)
        {
            Boolean added = _features.Add(feature);
            if (added) { feature.Initialise(this); ComponentAdded?.Invoke(feature); }
            return added;
        }


        /// <summary> Returns all features currently attached to this room, in unspecified order. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the room's internal feature set. </remarks>
        /// <returns> A read-only snapshot of every attached feature. </returns>
        public IReadOnlyCollection<Feature> GetComponents() => _features.GetAll();


        /// <summary> Checks whether a feature whose concrete runtime type is exactly <typeparamref name="TComponent"/> is currently attached. </summary>
        /// <typeparam name="TComponent"> The exact concrete feature type to test for. </typeparam>
        /// <returns> <c>true</c> if a feature of that exact type is attached; <c>false</c> otherwise. </returns>
        public Boolean HasComponent<TComponent>() where TComponent : Feature => _features.Has<TComponent>();


        /// <summary> Attempts to retrieve the attached feature whose concrete runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <remarks>
        /// Uses an exact-type dictionary lookup, not assignability scanning. A subclass of <typeparamref name="TComponent"/> stored under its
        /// own key will not be found here.
        /// </remarks>
        /// <typeparam name="TComponent"> The exact concrete feature type to retrieve. </typeparam>
        /// <param name="component"> Contains the attached feature when this method returns <c>true</c>; otherwise <c>default(<typeparamref name="TComponent"/>)</c>. </param>
        /// <returns> <c>true</c> if the matching feature was found; <c>false</c> if none is attached. </returns>
        public Boolean TryGetComponent<TComponent>(out TComponent component) where TComponent : Feature
        {
            return _features.TryGet<TComponent>(out component);
        }


        /// <summary> Removes a specific feature instance from this room. </summary>
        /// <param name="component"> The exact feature instance to detach. Must not be <c>null</c>. </param>
        /// <returns> <c>true</c> if the feature was removed; <c>false</c> if it was not attached or a different instance of the same type is attached. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="component"/> is <c>null</c>. </exception>
        /// <remarks>
        /// On success: calls <see cref="Feature.Detach"/> (which clears <c>Owner</c>) before raising
        /// <see cref="ComponentRemoved"/>, so <c>Owner</c> is already <c>null</c> when observers see the removal.
        /// </remarks>
        public Boolean RemoveComponent(Feature component)
        {
            Boolean removed = _features.Remove(component);
            if (removed) { component.Detach(); ComponentRemoved?.Invoke(component); }
            return removed;
        }


        /// <summary> Removes the attached feature whose runtime type is exactly <typeparamref name="TComponent"/>. </summary>
        /// <typeparam name="TComponent"> The type of feature to remove. </typeparam>
        /// <returns> <c>true</c> if a feature was removed; <c>false</c> if none was found. </returns>
        /// <remarks>
        /// On success: calls <see cref="Feature.Detach"/> (which clears <c>Owner</c>) before raising
        /// <see cref="ComponentRemoved"/>, so <c>Owner</c> is already <c>null</c> when observers see the removal.
        /// </remarks>
        public Boolean RemoveComponent<TComponent>() where TComponent : Feature
        {
            Boolean removed = _features.Remove<TComponent>(out Feature? removedComponent);
            if (removed) { removedComponent!.Detach(); ComponentRemoved?.Invoke(removedComponent); }
            return removed;
        }


        /// <summary> Registers a pre-constructed connection on this room. </summary>
        /// <remarks> Both endpoints of the connection must call this method independently; <see cref="WorldBuilder"/> handles both sides during the wiring pass. </remarks>
        /// <param name="connection"> The connection to register. </param>
        /// <returns> <c>true</c> if the connection was added; <c>false</c> if an equal connection was already registered. </returns>
        public Boolean AddConnection(Connection connection) => _connections.Add(connection);


        /// <summary> Returns a snapshot of all connections currently registered on this room. </summary>
        /// <returns> An immutable collection of connections; never null, but may be empty when no connections have been registered. </returns>
        public IReadOnlyCollection<Connection> GetConnections() => _connections.ToArray();


        /// <summary> Attempt to add an entity to the entities currently within the container. </summary>
        /// <param name="entity"> The entity to move into the container. </param>
        /// <param name="position"> The position within the room to place the entity. </param>
        /// <returns> <c>true</c> if the entity was successfully added; <c>false</c> if it was already present. </returns>
        public Boolean AddEntity(Entity entity, RoomPosition position)
        {
            Boolean result = false;
            if (!((IEntityContainer)this).Contains(entity))
            {
                _entities[position].Add(entity);
                result = true;
            }
            return result;
        }


        /// <summary> Attempt to remove an entity from the entities currently within the container. </summary>
        /// <param name="entity"> The entity to remove from the container. </param>
        /// <returns> Whether the entity was successfully removed from the container. </returns>
        public Boolean RemoveEntity(Entity entity) => _entities.Values.Any(x => x.Remove(entity));


        /// <inheritdoc/>
        public IReadOnlyCollection<Entity> GetEntities() => _entities.Values.SelectMany(x => x).ToArray();


        /// <summary> Returns a snapshot of the entities currently occupying a single position within the room. </summary>
        /// <param name="position"> The room position whose entities are to be retrieved. </param>
        /// <returns> An immutable snapshot of the entities at <paramref name="position"/>; never null, but may be empty when no entities occupy that position. </returns>
        public IReadOnlyCollection<Entity> GetEntities(RoomPosition position) => _entities[position].ToArray();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => UId.GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Room other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Room? other) => other is not null && UId.Equals(other.UId);
    }
}
