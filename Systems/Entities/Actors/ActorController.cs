using Godot;
using Khepri.Nodes.Singletons;
using Khepri.Types;
using System;

namespace Khepri.Entities.Actors
{
    /// <summary> A controller for actor entities within the game world. </summary>
    public partial class ActorController : SingletonNode3D<ActorController>
    {
        /// <summary> The prefab to use for creating beings. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _beingPrefab;


        /// <summary> A pool of instantiated beings to pull from first. </summary>
        public ObjectPool<ActorNode> ActorPool { get; private set; }
    }
}
