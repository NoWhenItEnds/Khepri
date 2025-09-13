using Godot;

namespace Khepri.Nodes
{
    /// <summary> An implementation of the sprite node specific to the project. </summary>
    public partial class KSprite3D : Sprite3D
    {
        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            SortingOffset = GlobalPosition.Z;
        }
    }
}
