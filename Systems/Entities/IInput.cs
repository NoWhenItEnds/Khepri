using Godot;

namespace Khepri.Entities
{
    /// <summary> The base class of an input data model. </summary>
    public interface IInput { }


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


    /// <summary> An input representing a movement in a direction. </summary>
    public record MoveInput : IInput
    {
        /// <summary> The direction of travel of a movement event. </summary>
        public Vector3 Direction { get; init; } = Vector3.Zero;

        /// <summary> The kind of movement that is being input / desired. </summary>
        public MoveType MovementType { get; init; } = MoveType.IDLE;


        /// <summary> An input representing a movement in a direction. </summary>
        /// <param name="direction"> The direction of travel of a movement event. </param>
        /// <param name="movementType"> The kind of movement that is being input / desired. </param>
        public MoveInput(Vector3 direction, MoveType movementType)
        {
            // Unless the unit is flying, ensure that is up-down axis is stripped.
            if (movementType != MoveType.FLYING) { direction = new Vector3(direction.X, 0f, direction.Z).Normalized(); }
            Direction = direction;
            MovementType = movementType;
        }


        /// <summary> The specific type of movement that is being carried out. </summary>
        public enum MoveType
        {
            IDLE,
            WALKING,
            SPRINTING,
            CROUCHING,
            CRAWLING,
            FLYING
        }
    }


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
