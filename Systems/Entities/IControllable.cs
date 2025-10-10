using Godot;
using Khepri.Models.Input;
using System;

namespace Khepri.Entities
{
    /// <summary> Represents an entity that is controllable by an agent. </summary>
    public interface IControllable
    {
        /// <summary> Handle the input sent to the entity by it's controller. </summary>
        /// <param name="input"> The input data class to interpret. </param>
        public void HandleInput(IInput input);
    }
}
