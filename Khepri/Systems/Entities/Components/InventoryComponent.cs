using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Components
{
    /// <summary> A component allowing an entity to hold other entities. </summary>
    /// <remarks> Initial contents are supplied by <c>InventoryDef</c> at spawn time, which instantiates the authored child prefabs and adds them via <see cref="AddEntity"/>. </remarks>
    public class InventoryComponent : Component, IEntityContainer
    {
        /// <summary> The entities currently stored in this inventory. </summary>
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();


        /// <summary> Initialises a new, empty <see cref="InventoryComponent"/>. </summary>
        /// <param name="entity"> The entity that has the ability to hold other entities. </param>
        public InventoryComponent(Entity entity) : base(entity)
        {
        }


        /// <inheritdoc/>
        public Boolean AddEntity(Entity entity)
        {
            Boolean wouldCycle = entity.Equals(ParentEntity)
                || entity.GetContainers().Any(c => c.Contains(ParentEntity));
            Boolean result = !wouldCycle && _entities.Add(entity);
            return result;
        }


        /// <inheritdoc/>
        public Boolean RemoveEntity(Entity entity) => _entities.Remove(entity);


        /// <inheritdoc/>
        public IReadOnlyCollection<Entity> GetEntities() => _entities.ToArray();
    }
}
