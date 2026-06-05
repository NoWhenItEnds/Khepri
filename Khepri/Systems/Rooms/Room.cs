using Khepri.Descriptions;
using Khepri.Entities;
using Khepri.Rooms.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Rooms
{
    /// <summary> A single self-contained playable space within the game world. It has entities within it, and connects to other rooms. </summary>
    public class Room : IEquatable<Room>, IEntityContainer
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

        /// <summary> The features that define the characteristics of this room, keyed by exact runtime type — a room holds at most one feature of each concrete type. Features are aspects of the room itself; entities are its contents — they are separate collections. </summary>
        private readonly Dictionary<Type, Feature> _features = new Dictionary<Type, Feature>();


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
            List<Action<DescriptionBuilder>> contributions = new List<Action<DescriptionBuilder>>();

            foreach (Feature feature in _features.Values.OrderBy(feature => feature.Order))
            {
                contributions.Add(feature.Contribute);
            }

            foreach (Entity entity in GetEntities())
            {
                contributions.Add(entity.Contribute);
            }

            Description description = DescriptionBuilder.Compose(contributions);

            return description.Spans.Count > 0
                ? description
                : new DescriptionBuilder().Text("This is a room.").Build();
        }


        /// <summary> Adds a feature instance to this room. </summary>
        /// <param name="feature"> The feature being added. </param>
        /// <returns> <c>true</c> if the feature was added; <c>false</c> if a feature of the same concrete type already exists. </returns>
        public Boolean AddFeature(Feature feature) => _features.TryAdd(feature.GetType(), feature);


        /// <summary> Gets the first attached feature assignable to <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The kind of feature to retrieve. </typeparam>
        /// <returns> The feature instance, or <c>null</c> if none is attached. </returns>
        public T? GetFeature<T>() where T : Feature => _features.Values.OfType<T>().FirstOrDefault();


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
