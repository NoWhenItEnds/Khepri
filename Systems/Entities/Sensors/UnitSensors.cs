using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Sensors
{
    /// <summary> The controller or root node of a unit's sensors. </summary>
    public partial class UnitSensors : Node3D
    {
        /// <summary> The array of entities currently 'known' by the sensors. </summary>
        private HashSet<KnownEntity> _trackedEntities = new HashSet<KnownEntity>();


        /// <summary> Adds an entity to the ones being tracked by the unit. Represents its memory. </summary>
        /// <param name="entity"> The object to begin tracking. </param>
        /// <returns> Whether the object was successfully tracked. </returns>
        public Boolean RememberEntity(ISmartEntity entity)
        {
            return _trackedEntities.Add(new KnownEntity(entity));
        }


        /// <summary> Stops tracking the given entity. </summary>
        /// <param name="entity"> A reference to the object to stop tracking. </param>
        /// <returns> Whether the object was removed. </returns>
        public Boolean ForgetEntity(ISmartEntity entity)
        {
            return _trackedEntities.RemoveWhere(x => x.SmartEntity == entity) > 0;
        }


        /// <summary> Searches the tracked entity's for a particular smart object. </summary>
        /// <param name="entity"> A reference to the tracked entity. A null means that one wasn't found. </param>
        public KnownEntity? FindEntity(ISmartEntity entity)
        {
            return _trackedEntities.FirstOrDefault(x => x.SmartEntity == entity);
        }


        /// <summary> Searches the tracked entities for an entity of a particular type. This is to search for a kind rather than a specific instance. </summary>
        /// <param name="entity"> An array of entities sharing the given type. An empty array indicates that there are none of the desired type. </param>
        public KnownEntity[] FindTypeOfEntity(ISmartEntity entity)
        {
            return _trackedEntities.Where(x => x.SmartEntity.GetType() == entity.GetType()).ToArray();
        }
    }
}
