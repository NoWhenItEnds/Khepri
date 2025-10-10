using Khepri.Entities;

namespace Khepri.Models.Input
{
    /// <summary> An input representing an attempt to use an entity. </summary>
    public record UseInput : IInput
    {
        /// <summary> The entity the action is trying to use. </summary>
        public IEntity Entity { get; init; }


        /// <summary> An input representing an attempt to use and entity. </summary>
        /// <param name="entity"> The entity the action is trying to use. </param>
        public UseInput(IEntity entity)
        {
            Entity = entity;
        }
    }
}
