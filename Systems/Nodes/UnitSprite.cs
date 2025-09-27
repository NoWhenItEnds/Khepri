using System;
using Godot;
using Khepri.Entities.UnitComponents.States;
using Khepri.Types;

namespace Khepri.Nodes
{
    /// <summary> The layered animated sprites used to render a unit. </summary>
    public partial class UnitSprite : Node3D
    {
        /// <summary> A reference to the animated sprite used for rendering the unit's base layer. </summary>
        [ExportGroup("Sprite Nodes")]
        [Export] private AnimatedSprite3D _baseLayer;

        /// <summary> The previously registered direction. </summary>
        private Direction _previousDirection = Direction.S;


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            _baseLayer.SortingOffset = (Int32)GlobalPosition.Y * 10000 + GlobalPosition.Z;
        }


        /// <summary> Begin playing all the unit's animations. </summary>
        public void Play()
        {
            _baseLayer.Play();
        }


        /// <summary> Stop playing all the unit's animations. </summary>
        public void Stop()
        {
            _baseLayer.Stop();
        }


        /// <summary> Sets the sprites of a particular layer. </summary>
        /// <param name="layer"> The layer to update. </param>
        /// <param name="sprites"> The new sprite sheet animation. </param>
        public void SetSpriteLayer(UnitSpriteLayer layer, SpriteFrames sprites)
        {
            switch (layer)
            {
                case UnitSpriteLayer.BASE:
                    _baseLayer.SpriteFrames = sprites;
                    break;
            }
        }


        /// <summary> Transitions the unit's animations to a new one. </summary>
        /// <param name="state"> The current state of the unit. </param>
        /// <param name="direction"> The direction the unit is currently facing. </param>
        public void TransitionAnimation(UnitState state, Direction direction = Direction.NONE)
        {
            _previousDirection = direction != Direction.NONE ? direction : _previousDirection;
            String animationName = state.AnimationPrefix + _previousDirection;
            _baseLayer.Animation = _baseLayer.SpriteFrames.HasAnimation(animationName) ? animationName : throw new ArgumentException($"Animation '{animationName}' does not exist in the sprite frames!");
        }
    }


    /// <summary> The layer of a particular sprite on a unit. </summary>
    public enum UnitSpriteLayer
    {
        NONE,
        BASE
    }
}
