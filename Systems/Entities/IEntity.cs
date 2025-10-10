using Godot;
using Khepri.Entities.Actors;

namespace Khepri.Entities
{
    /// <summary> The entity is persistent, meaning that its information should be remembered between game sessions. </summary>
    public interface IEntity
    {
        /// <summary> The location of the entity in world space. </summary>
        public Vector3 WorldPosition { get; }

        /// <summary> A reference to the object's collision shape. </summary>
        public CollisionShape3D CollisionShape { get; }


        /// <summary> The internal logic to use when the entity is examined. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public void Examine(Unit activatingEntity);


        /// <summary> The internal logic to use when the entity is used. </summary>
        /// <param name="activatingEntity"> A reference to the unit activating the action. </param>
        public void Use(Unit activatingEntity);
    }
}
