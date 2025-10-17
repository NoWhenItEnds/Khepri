using Godot;
using Khepri.Resources;
using Khepri.Types;

namespace Khepri.Entities.Actors
{
    public abstract partial class ActorNode : CharacterBody3D, IPoolable
    {
        /// <inheritdoc/>
        public abstract T GetResource<T>() where T : EntityResource;


        /// <inheritdoc/>
        public void FreeObject() => ActorController.Instance.ActorPool.FreeObject(this);
    }
}
