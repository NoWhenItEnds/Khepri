using Godot;
using System;

namespace Khepri.Nodes
{
    /// <summary> An implementation of an animated sprite that uses position to modify its sort order. </summary>
    public partial class LayeredAnimatedSprite3D : AnimatedSprite3D
    {
        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            SortingOffset = (Int32)GlobalPosition.Y * 10000 + GlobalPosition.Z;
        }
    }
}
