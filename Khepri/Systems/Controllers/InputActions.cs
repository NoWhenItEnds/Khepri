using Godot;

namespace Khepri.Controllers
{
    /// <summary> The names of the actions defined in the project's input map. </summary>
    public static class InputActions
    {
        /// <summary> Burn along the ship's heading. </summary>
        public static readonly StringName ForwardThrust = new StringName("action_forward_thrust");

        /// <summary> Burn in the opposite direction of the ship's heading. </summary>
        public static readonly StringName ReverseThrust = new StringName("action_reverse_thrust");

        /// <summary> Turn counter-clockwise. </summary>
        public static readonly StringName RotateLeft = new StringName("action_rotate_left");

        /// <summary> Turn clockwise. </summary>
        public static readonly StringName RotateRight = new StringName("action_rotate_right");
    }
}
