using Godot;
using System;

namespace Khepri.Nodes
{
    /// <summary> The navigation region for a given Z-level. </summary>
    public partial class Level : NavigationRegion3D
    {
        /// <inheritdoc/>
        public override void _Ready()
        {
            Name = $"Level - {(Int32)GlobalPosition.Y:000}";
        }
    }
}
