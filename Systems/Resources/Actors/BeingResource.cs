using Godot;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Entities.Actors.Components.States;
using System;

namespace Khepri.Resources.Actors
{
    /// <summary> The data component for a living creature within the game world. </summary>
    [GlobalClass]
    public partial class BeingResource : ActorResource
    {
        /// <summary> A state machine for being states. </summary>
        [ExportGroup("Statistics")]
        [Export] public BeingStateMachine StateMachine { get; private set; } = new BeingStateMachine();

        /// <summary> A being's needs and desires. </summary>
        [Export] public BeingNeedsResource Needs { get; private set; } = new BeingNeedsResource();


        /// <summary> The data component for a living creature within the game world. </summary>
        public BeingResource() {}


        /// <summary> Initialise the resource. </summary>
        /// <param name="being"> A reference to the being this resource represents. </param>
        public void Initialise(Being being)
        {
            StateMachine.Initialise(being);
        }


        /// <inheritdoc/>
        public void HandleInput(IInput input)
        {
            throw new NotImplementedException();
        }
    }
}
