using Godot;
using Khepri.Entities;
using System;

namespace Khepri.Navigation
{
    /// <summary> The digital agent used to control a unit. </summary>
    public partial class AIController : Node
    {
        /// <summary> The unit this controller will control. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _unit;

        private Single _speed = 5f;

        private Single _accuracy = 1f;

        private Single _turnSpeed = 5f;


        /// <summary> The index of the current destination node along the path. </summary>
        private Int32 _currentDestinationPathIndex;


        /// <inheritdoc/>
        public override void _Ready()
        {

        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
        }
    }
}
