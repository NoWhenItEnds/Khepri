using Godot;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Entities
{
    /// <summary> The entity is persistent, meaning that its information should be remembered between game sessions. </summary>
    public interface IEntity
    {
        /// <summary> The object's unique identifier. Acts as it's key value when mapping data to it. </summary>
        public Guid UId { get; }

        /// <summary> The location of the entity in world space. </summary>
        public Vector3 WorldPosition { get; }

        /// <summary> A reference to the object's collision shape. </summary>
        public CollisionShape3D CollisionShape { get; }


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        /// <returns> If the entity was successfully examined. </returns>
        public Boolean Examine(Unit activatingEntity);


        /// <summary> The internal logic to use when the entity is picked up. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        /// <returns> If the entity was successfully picked up. </returns>
        public Boolean Grab(Unit activatingEntity);


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        /// <returns> If the entity was successfully used. </returns>
        public Boolean Use(Unit activatingEntity);
    }
}
