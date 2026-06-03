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

        /// <summary> The entities directly in the current room, not those nested within container components. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();

        /// <summary> The features that define the characteristics of this room. Features are aspects of the room itself; entities are its contents — they are separate collections. </summary>
        private readonly HashSet<Feature> _features = new HashSet<Feature>();


        /// <summary> Initialises a new instance of the <see cref="Room"/> class. </summary>
        /// <param name="uid"> The unique identifier for the room. </param>
        public Room(Guid uid)
        {
            UId = uid;
        }


        /// <summary> Builds a dynamic description of the room's current state to display to the player. </summary>
        /// <remarks> Delegates to an attached <see cref="DescriptionFeature"/> when one is present; otherwise returns a sensible default. </remarks>
        /// <returns> A formatted string representing the room's current state. </returns>
        public String BuildDescription()
        {
            DescriptionFeature? descriptionFeature = GetFeature<DescriptionFeature>();
            Boolean hasDescription                 = descriptionFeature is not null;

            String result = hasDescription
                ? descriptionFeature!.Text
                : "This is a room.";

            return result;
        }


        /// <summary> Adds a pre-constructed feature instance to this room. </summary>
        /// <param name="feature"> The feature being added. </param>
        /// <returns> <c>true</c> if the feature was added; <c>false</c> if an equal feature already exists. </returns>
        public Boolean AddFeature(Feature feature) => _features.Add(feature);


        /// <summary> Gets the first attached feature of type <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The kind of feature to retrieve. </typeparam>
        /// <returns> The feature instance, or <c>null</c> if none is attached. </returns>
        public T? GetFeature<T>() where T : Feature => _features.OfType<T>().FirstOrDefault();


        /// <summary> Registers a pre-constructed connection on this room. </summary>
        /// <remarks> Both endpoints of the connection must call this method independently; <see cref="WorldBuilder"/> handles both sides during the wiring pass. </remarks>
        /// <param name="connection"> The connection to register. </param>
        /// <returns> <c>true</c> if the connection was added; <c>false</c> if an equal connection was already registered. </returns>
        public Boolean AddConnection(Connection connection) => _connections.Add(connection);


        /// <summary> Returns a snapshot of all connections currently registered on this room. </summary>
        /// <returns> An immutable collection of connections; never null, but may be empty when no connections have been registered. </returns>
        public IReadOnlyCollection<Connection> GetConnections() => _connections.ToArray();


        /// <inheritdoc/>
        public Boolean AddEntity(Entity entity) => _entities.Add(entity);


        /// <inheritdoc/>
        public Boolean RemoveEntity(Entity entity) => _entities.Remove(entity);


        /// <inheritdoc/>
        public IReadOnlyCollection<Entity> GetEntities() => _entities.ToArray();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => UId.GetHashCode();


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj) => obj is Room other && Equals(other);


        /// <inheritdoc/>
        public Boolean Equals(Room? other) => other is not null && UId.Equals(other.UId);
    }
}
