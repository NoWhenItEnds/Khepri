using System;
using System.Collections.Generic;
using Jaypen.Utilities.Singletons;
using Khepri.Entities.Actions;
using Khepri.Entities.Controllers;
using Khepri.Rooms;

namespace Khepri.Managers
{
    /// <summary> Drives the game's turn order: each frame it lets ready controllers take their turn until it must wait on the player. </summary>
    /// <remarks>
    /// Turns are strict round-robin over the participating controllers, the player first. A controller takes its turn by
    /// deciding an <see cref="EntityAction"/> (which self-performs); the turn is spent unless that action fails, so an
    /// illegal player move lets the player choose again rather than wasting their turn. The player is asynchronous: the
    /// loop pauses on the player's turn until a choice has been queued (<see cref="Controllers.EntityController.IsReady"/>),
    /// whereas AI controllers are always ready and simply pass when they have nothing to do.
    /// </remarks>
    public partial class TurnManager : SingletonNode<TurnManager>
    {
        /// <summary> The controller driving the human player; the turn order always starts here. </summary>
        public PlayerController Player { get; private set; } = null!;


        /// <summary> The participating controllers in turn order, the player at index 0. </summary>
        private readonly List<EntityController> _order = new List<EntityController>();

        /// <summary> The index into <see cref="_order"/> of the controller whose turn it currently is. </summary>
        private Int32 _index;


        /// <summary> Builds the turn order from the given participants, placing the player first. </summary>
        /// <param name="participants"> Every controller that should take turns; the player may appear among them and is not duplicated. </param>
        /// <param name="player"> The player's controller, which takes the first turn of each round. </param>
        public void Initialise(IEnumerable<EntityController> participants, PlayerController player)
        {
            Player = player;
            _order.Clear();
            _order.Add(player);

            foreach (EntityController participant in participants)
            {
                if (!ReferenceEquals(participant, player))
                {
                    _order.Add(participant);
                }
            }

            _index = 0;
        }


        /// <inheritdoc/>
        public override void _Process(Double delta)
        {
            Int32   processed = 0;
            Boolean blocked   = false;

            // Advance whole turns until the player needs to act (blocked) or a full round has been taken this frame
            // (the round cap also stops a runaway loop if no controller is ever blocked).
            while (!blocked && processed < _order.Count)
            {
                EntityController current = _order[_index];

                if (current.IsReady)
                {
                    Boolean spent = TakeTurn(current);

                    if (spent)
                    {
                        _index = (_index + 1) % _order.Count;
                    }

                    processed++;
                }
                else
                {
                    blocked = true;
                }
            }
        }


        /// <summary> Lets <paramref name="controller"/> decide and perform its action, reporting whether the turn was spent. </summary>
        /// <remarks> A turn is spent when the controller passes (no action) or its action takes effect; a failed action does not spend the turn, so the actor may try again. </remarks>
        /// <param name="controller"> The controller taking its turn. </param>
        /// <returns> <c>true</c> if the turn was spent and play should move on; <c>false</c> to leave the turn with this controller. </returns>
        private static Boolean TakeTurn(EntityController controller)
        {
            Boolean spent = true;
            Room?   room  = RoomManager.Instance!.GetCurrentRoom(controller.Entity);

            if (room is not null)
            {
                EntityAction? action = controller.Act(room);

                if (action is not null)
                {
                    spent = action.Perform() != ActionResult.Failed;
                }
            }

            return spent;
        }
    }
}
