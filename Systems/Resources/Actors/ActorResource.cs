using Godot;

namespace Khepri.Resources.Actors
{
    public abstract partial class ActorResource : EntityResource
    {
        /// <summary> A reference to the sprites the actor uses in the world. </summary>
        [ExportGroup("Sprites")]
        [Export] public SpriteFrames WorldSprites { get; private set; }

        public ActorResource() { }
    }
}
