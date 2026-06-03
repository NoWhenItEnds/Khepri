using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> Owns every entity's controller and drives the world clock. Time is player-driven: the world advances one step each time the player commits an action. </summary>
    /// <remarks>
    /// This is the home for the things that are neither entity-construction (EntityManager) nor world-geometry (RoomManager): the mapping of entity to brain, the privileged <see cref="Player"/> pointer, and the turn loop.
    /// "The player" is simply <c>Player.Owner</c>. Controllers are registered here when their entity enters the world and removed when it leaves.
    /// </remarks>
    public partial class TurnManager : SingletonNode<TurnManager>
    {
        /// <summary> The amount of in-game time that elapses per completed turn. </summary>
        private static readonly TimeSpan TurnDuration = TimeSpan.FromMinutes(1);

        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<TurnManager>();


        /// <summary> Every controller in the world, keyed by the entity it drives, so a controller can be found and removed by entity. </summary>
        private readonly Dictionary<Entity, EntityController> _controllers = new Dictionary<Entity, EntityController>();


        /// <summary> The player's controller; <c>Player.Owner</c> is the player entity. <c>null</c> until the player is registered. </summary>
        public PlayerController? Player { get; private set; }

        /// <summary> The number of turns that have elapsed since the world began. </summary>
        public Int64 TurnNumber { get; private set; }


        /// <inheritdoc/>
        public override void _Ready()
        {
            SetProcessUnhandledInput(true);
        }


        /// <summary> Registers the player's controller and records it as the privileged <see cref="Player"/>. </summary>
        /// <param name="controller"> The player's controller. </param>
        public void SetPlayer(PlayerController controller)
        {
            Player = controller;
            _controllers[controller.Owner] = controller;
        }


        /// <summary> Registers a (non-player) controller so its entity acts each world step. </summary>
        /// <param name="controller"> The controller to register; replaces any existing controller for the same entity. </param>
        public void Register(EntityController controller)
        {
            _controllers[controller.Owner] = controller;
        }


        /// <summary> Removes the controller driving <paramref name="entity"/>, e.g. when it is destroyed. </summary>
        /// <param name="entity"> The entity whose controller should stop acting. </param>
        public void Deregister(Entity entity)
        {
            _controllers.Remove(entity);
        }


        /// <inheritdoc/>
        public override void _UnhandledInput(InputEvent @event)
        {
            Boolean isAdvance = @event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Space };

            if (isAdvance)
            {
                TakePlayerTurn();
                GetViewport().SetInputAsHandled();
            }
        }


        /// <summary> Resolves the player's action and then advances the world for every other actor, elapsing one turn of game time. </summary>
        private void TakePlayerTurn()
        {
            if (Player is null)
            {
                return;
            }

            ActOne(Player);
            AdvanceWorld();

            TurnNumber++;
            GameManager.Instance!.AdvanceTime(TurnDuration);

            Logger.LogInformation("Turn {Turn} complete.", TurnNumber);
        }


        /// <summary> Asks every non-player controller to act once. </summary>
        /// <remarks> Iterates a snapshot so a controller may register or deregister others (spawn/death) without invalidating the loop. </remarks>
        private void AdvanceWorld()
        {
            List<EntityController> actors = _controllers.Values.Where(controller => controller != Player).ToList();

            foreach (EntityController actor in actors)
            {
                ActOne(actor);
            }
        }


        /// <summary> Looks up the room a controller's entity is in and invokes its action there. </summary>
        /// <param name="controller"> The controller to run. </param>
        private static void ActOne(EntityController controller)
        {
            Room room = RoomManager.Instance!.GetCurrentRoom(controller.Owner);
            controller.Act(room);
        }
    }
}
