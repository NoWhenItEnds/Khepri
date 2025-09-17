using Godot;
using Khepri.Entities.Interfaces;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Entities.Sensors
{
    /// <summary> The controller or root node of a unit's sensors. </summary>
    public partial class UnitSensors : Node3D
    {
        /// <summary> Whether the debug tools should be shown. </summary>
        [ExportGroup("Debug")]
        [Export] private Boolean _showDebug = false;


        /// <summary> The array of entities currently 'known' by the sensors. </summary>
        private HashSet<KnownEntity> _trackedEntities = new HashSet<KnownEntity>();


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            // Render debug gizmos.
            if (_showDebug)
            {
                foreach (KnownEntity entity in _trackedEntities)
                {
                    DebugDraw3D.DrawAabb(entity.SmartEntity.CollisionShape.GetAabb(),
                        entity.IsVisible ? Colors.DarkViolet : Colors.MediumPurple);
                }
            }
        }


        /// <summary> Adds an entity to the ones being tracked by the unit. Represents its memory. </summary>
        /// <param name="entity"> The object to begin tracking. </param>
        /// <returns> A reference to the newly remembered know entity. </returns>
        public KnownEntity RememberEntity(ISmartEntity entity)
        {
            KnownEntity newEntity = new KnownEntity(entity);
            _trackedEntities.Add(newEntity);
            return newEntity;
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


    /// <summary> A data object that is 'known' by an agent. </summary>
    public class KnownEntity
    {
        /// <summary> A reference to the known entity. </summary>
        public ISmartEntity SmartEntity { get; init; }

        /// <summary> Whether the entity is visible. </summary>
        public Boolean IsVisible { get; private set; } = false;

        /// <summary> The last known position of the entity. </summary>
        /// <remarks> A null value means that one isn't known. </remarks>
        public Vector3 LastKnownPosition { get; private set; }


        /// <summary> A data object that is 'known' by an agent. </summary>
        /// <param name="entity"> A reference to the known entity. </param>
        public KnownEntity(ISmartEntity entity)
        {
            SmartEntity = entity;
            LastKnownPosition = entity.CollisionShape.GlobalPosition;
        }


        /// <summary> Set whether the entity is currently visible to the tracker. </summary>
        /// <param name="isVisible"> Whether the smart entity is directly visible. </param>
        public void SetIsVisible(Boolean isVisible)
        {
            IsVisible = isVisible;
        }


        /// <summary> Updates the last known position of the entity. </summary>
        /// <param name="position"> An optional position. A null will use the smart entity's actual position, a value will override it. </param>
        public void UpdateLastKnownPosition(Vector3? position = null)
        {
            LastKnownPosition = position == null ? SmartEntity.CollisionShape.GlobalPosition : position.Value;
        }
    }
}
