using System.Collections.Generic;
using Godot;
using Jaypen.Logging;
using Jaypen.Singletons;
using Khepri.Controllers;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's manager for all entity controllers. </summary>
    public partial class ControllerManager : SingletonNode<ControllerManager>
    {
        /// <summary> A reference to the controller used by the player to interact with the game world. </summary>
        [ExportGroup("Nodes")]
        [Export] public PlayerController PlayerController { get; private set; } = null!;


        /// <summary> The array of all AI controllers currently within the game world. </summary>
        private readonly HashSet<EntityController> _aiControllers = new HashSet<EntityController>();


        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<ControllerManager>();


        /// <inheritdoc/>
        public override void _Ready()
        {
        }
    }
}
