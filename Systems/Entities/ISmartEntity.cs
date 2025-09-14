using Godot;

namespace Khepri.Entities
{
    /// <summary> An object that can be interacted with by an AI agent. </summary>
    public interface ISmartEntity : IPersistent
    {
        /// <summary> A reference to the object's collision shape. </summary>
        public CollisionShape3D CollisionShape { get; }
    }
}
