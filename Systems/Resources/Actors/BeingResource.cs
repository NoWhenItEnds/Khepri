using Godot;
using Khepri.Entities;
using System;

namespace Khepri.Resources.Actors
{
    /// <summary> The data component for a living creature within the game world. </summary>
    [GlobalClass]
    public partial class BeingResource : ActorResource
    {
        /// <summary> A being's needs and desires. </summary>
        [ExportGroup("Statistics")]
        [Export] public BeingNeedsResource Needs { get; private set; } = new BeingNeedsResource();


        /// <summary> The data component for a living creature within the game world. </summary>
        public BeingResource() { }


        /// <inheritdoc/>
        public void HandleInput(IInput input)
        {
            throw new NotImplementedException();
        }
    }
}
