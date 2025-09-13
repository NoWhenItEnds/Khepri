using Godot;

namespace Khepri.Models.Input
{
    /// <summary> An input representing a movement in a direction. </summary>
    public record MoveInput : IInput
    {
        /// <summary> The direction of travel of a movement event. </summary>
        public Vector3 Direction { get; init; } = Vector3.Zero;

        /// <summary> The kind of movement that is being input / desired. </summary>
        public MoveType MovementType { get; init; } = MoveType.NONE;


        /// <summary> An input representing a movement in a direction. </summary>
        /// <param name="direction"> The direction of travel of a movement event. </param>
        /// <param name="movementType"> The kind of movement that is being input / desired. </param>
        public MoveInput(Vector3 direction, MoveType movementType)
        {
            Direction = direction;
            MovementType = movementType;
        }
    }


    /// <summary> The specific type of movement that is being carried out. </summary>
    public enum MoveType
    {
        NONE,
        WALKING,
        SPRINTING,
        CROUCHING,
        CRAWLING,
        FLYING
    }
}
