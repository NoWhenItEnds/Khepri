using System;
using Godot;
using Khepri.Entities.Actors;
using Khepri.Resources;

namespace Khepri.Entities
{
    /// <summary> The entity is persistent, meaning that its information should be remembered between game sessions. </summary>
    public interface IEntity
    {
        /// <summary> Get the entity's current position within the game world. </summary>
        public Vector3 GetWorldPosition();


        /// <summary> Get the entity's bounding box. </summary>
        public CollisionShape3D GetCollisionShape();


        /// <summary> Gets the data resource associated with the entity cast to the given type. </summary>
        /// <typeparam name="T"> The type of resource desired. </typeparam>
        /// <returns> The internal resource cast to the given type. </returns>
        /// <exception cref="InvalidCastException"></exception>
        public T GetResource<T>() where T : EntityResource;


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public void Examine(Being activatingEntity);


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public void Use(Being activatingEntity);
    }
}
