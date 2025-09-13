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
}
