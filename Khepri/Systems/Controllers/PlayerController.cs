using System;
using Godot;
using Khepri.World;
using Khepri.World.Entities;

namespace Khepri.Controllers
{
    /// <summary> Owns the player's presence in the world: spawns the player's ship and possesses it with the player's input-map controller. The flying itself happens through the intent seam — the simulation polls the controller like it would any AI's. </summary>
    public partial class PlayerController : EntityController
    {
        /// <inheritdoc/>
        public override void _Input(InputEvent @event)
        {
            if (Entity != null && Entity is Ship ship)
            {
                Single throttle = Input.GetActionStrength(InputActions.ForwardThrust);
                Single steer = Input.GetAxis(InputActions.RotateLeft, InputActions.RotateRight);
                ActionIntent intent = new ActionIntent(throttle, steer);

                ship.HandleInput(intent);
            }

        }
    }
}
