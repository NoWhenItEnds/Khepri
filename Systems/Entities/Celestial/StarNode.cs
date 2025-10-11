using Godot;
using Khepri.Controllers;
using Khepri.Resources.Celestial;
using Khepri.Types;
using System;

namespace Khepri.Entities.Celestial
{
    /// <summary> A node in the UI representing a star node. </summary>
    /// <remarks> This doesn't need to be a star, lol, just the root celestial object that things could be orbiting around. </remarks>
    public partial class StarNode : Node, IPoolable<StarResource>
    {
        /// <inheritdoc/>
        [ExportGroup("Statistics")]
        [Export] public StarResource Resource { get; set; }


        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;


        /// <inheritdoc/>
        public void FreeObject() => throw new NotImplementedException();
    }
}
