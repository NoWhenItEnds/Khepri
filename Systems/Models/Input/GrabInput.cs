using Khepri.Entities;

namespace Khepri.Models.Input
{
    /// <summary> An input representing an attempt to pick an entity up. </summary>
    public record GrabInput : IInput
    {
        /// <summary> The entity the action is trying to grab. </summary>
        public IEntity Entity { get; init; }


        /// <summary> An input representing an attempt to pick an entity up. </summary>
        /// <param name="entity"> The entity the action is trying to grab. </param>
        public GrabInput(IEntity entity)
        {
            Entity = entity;
        }
    }
}
