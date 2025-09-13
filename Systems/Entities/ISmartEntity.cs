using System;
using Godot;

namespace Khepri.Entities
{
    /// <summary> An object that can be interacted with by an AI agent. </summary>
    public interface ISmartEntity
    {
        /// <summary> The object's unique identifier. Acts as it's key value when mapping data to it. </summary>
        public Guid UId { get; }

        /// <summary> A reference to the object's collision shape. </summary>
        public CollisionShape3D CollisionShape { get; }
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
