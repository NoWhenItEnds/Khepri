using Godot;
using System;

namespace Khepri.Entities
{
    /// <summary> The entity is persistent, meaning that its information should be remembered between game sessions. </summary>
    public interface IEntity : IComparable<IEntity>
    {
        /// <summary> The object's unique identifier. Acts as it's key value when mapping data to it. </summary>
        public Guid UId { get; }

        /// <summary> The location of the entity in world space. </summary>
        public Vector3 WorldPosition { get; }

        /// <summary> A reference to the object's collision shape. </summary>
        public CollisionShape3D CollisionShape { get; }
    }
}
