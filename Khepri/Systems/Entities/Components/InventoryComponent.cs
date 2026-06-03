using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Components
{
    /// <summary> A component allowing an entity to hold other entities. </summary>
    public class InventoryComponent : Component
    {
        /// <summary> The entities currently stored in this inventory. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();


        /// <summary> Initialises a new instance of the <see cref="InventoryComponent"/> class. </summary>
        /// <param name="entity"> The entity that has the ability to hold other entities. </param>
        public InventoryComponent(Entity entity) : base(entity)
        {
        }


        /// <summary> Attempt to add an entity to the entities currently within the inventory. </summary>
        /// <param name="entity"> The entity to move into the inventory. </param>
        /// <returns> Whether the entity was successfully added to the inventory. </returns>
        public Boolean AddEntity(Entity entity) => _entities.Add(entity);


        /// <summary> Attempt to remove an entity from the entities currently within the inventory. </summary>
        /// <param name="entity"> The entity to remove from the inventory. </param>
        /// <returns> Whether the entity was successfully removed to the inventory. </returns>
        public Boolean RemoveEntity(Entity entity) => _entities.Remove(entity);


        /// <summary> Get an immutable array of the entities within this inventory. </summary>
        /// <returns> The entities currently in this inventory. </returns>
        public Entity[] GetEntities() => _entities.ToArray();
    }
}
