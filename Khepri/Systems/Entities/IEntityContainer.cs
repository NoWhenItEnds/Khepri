using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities
{
    /// <summary> Represents any object capable of directly holding a collection of entities — for example, a room or an inventory-style component. </summary>
    /// <remarks> An entity is expected to be held by at most one container at a time. Callers must remove an entity from its current container before placing it in a new one; cycle-detection in implementations such as <see cref="Components.InventoryComponent.AddEntity"/> relies on this invariant. </remarks>
    public interface IEntityContainer
    {
        /// <summary> Returns the entities directly held by this container, one level deep. </summary>
        /// <remarks> The returned collection is a snapshot — mutations to it do not affect the container's internal state. </remarks>
        /// <returns> A read-only snapshot of the directly-contained entities. </returns>
        public IReadOnlyCollection<Entity> GetEntities();


        /// <summary> Attempt to add an entity to the entities currently within the container. </summary>
        /// <param name="entity"> The entity to move into the container. </param>
        /// <returns> <c>true</c> if the entity was successfully added; <c>false</c> if it was already present or if adding it would create a containment cycle (e.g. placing the owning entity, or any ancestor in its container tree, into this container). </returns>
        public Boolean AddEntity(Entity entity);


        /// <summary> Attempt to remove an entity from the entities currently within the container. </summary>
        /// <param name="entity"> The entity to remove from the container. </param>
        /// <returns> Whether the entity was successfully removed from the container. </returns>
        public Boolean RemoveEntity(Entity entity);


        /// <summary> Determines whether the given entity exists within this container, either directly or nested inside any container component of a held entity. </summary>
        /// <param name="target"> The entity to search for. </param>
        /// <returns> <c>true</c> if <paramref name="target"/> is contained anywhere within this container's tree; otherwise <c>false</c>. </returns>
        /// <remarks> Short-circuits on first match. Implementations may override this default for a faster search (e.g. a container backed by a reverse index). </remarks>
        public Boolean Contains(Entity target) =>
            GetEntities().Any(held =>
                held.Equals(target) ||
                held.GetContainers().Any(c => c.Contains(target)));
    }
}
