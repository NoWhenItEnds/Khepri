using Khepri.Entities;

namespace Khepri.Models.Input
{
    /// <summary> An input representing an attempt to examine an entity. </summary>
    public record ExamineInput : IInput
    {
        /// <summary> The entity the action is trying to examine. </summary>
        public IEntity Entity { get; init; }


        /// <summary> An input representing an attempt to examine an entity. </summary>
        /// <param name="entity"> The entity the action is trying to examine. </param>
        public ExamineInput(IEntity entity)
        {
            Entity = entity;
        }
    }
}
