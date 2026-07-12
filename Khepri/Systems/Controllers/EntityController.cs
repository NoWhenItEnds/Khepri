using Godot;
using Khepri.World.Entities;

namespace Khepri.Controllers
{
    /// <summary> Provides input, whether player or AI driven, to an entity. </summary>
    public abstract partial class EntityController : Node
    {
        /// <summary> The entity the controller is currently manipulating. </summary>
        public Entity? Entity { get; protected set; } = null;


        /// <summary> Set the target of the controller. The entity it is controlling. </summary>
        /// <param name="ship"> The new entity the controller will target. </param>
        public virtual void SetEntity(Entity entity)
        {
            Entity = entity;
        }
    }
}
