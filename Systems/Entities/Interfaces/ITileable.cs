using Godot;
using System;

namespace Khepri.Entities.Interfaces
{
    /// <summary> Indicates that the entity is tiling, and should consider other nearby examples of itself to determine is qualities. </summary>
    public interface ITileable
    {
        /// <summary> Sets the object's sprite by looking at those around it. </summary>
        protected void SetSprite()
        {

        }
    }
}
