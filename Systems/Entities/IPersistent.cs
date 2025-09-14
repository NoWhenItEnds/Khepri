using Godot;
using System;

namespace Khepri.Entities
{
    /// <summary> The entity is persistent, meaning that its information should be remembered between game sessions. </summary>
    public interface IPersistent : IComparable<IPersistent>
    {
        /// <summary> The object's unique identifier. Acts as it's key value when mapping data to it. </summary>
        public Guid UId { get; }

        /// <summary> The location of the entity in world space. </summary>
        public Vector3 WorldPosition { get; }

        /// <summary> The navigation cost of moving through the entity. </summary>
        /// <remarks> -1 means that is shouldn't be included in the navigation calculations. </remarks>
        public Single NavigationCost { get; }
    }
}
