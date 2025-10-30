using Godot;
using Khepri.Controllers;
using Khepri.Data;
using Khepri.Data.Actors;
using Khepri.Nodes.Singletons;
using Khepri.Types;
using System;

namespace Khepri.Entities.Actors
{
    /// <summary> A controller for actor entities within the game world. </summary>
    public partial class ActorController : SingletonNode3D<ActorController>
    {
        /// <summary> The prefab to use for creating actors. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _actorPrefab;


        /// <summary> The node to parent controllers to. </summary>
        [ExportGroup("Nodes")]
        [Export] private Node _controllerParent;

        /// <summary> The node to parent actor nodes to. </summary>
        [Export] private Node _actorParent;

        /// <summary> A reference to the player's controller. </summary>
        [Export] private PlayerController _playerController;


        /// <summary> A pool of instantiated actors to pull from first. </summary>
        public ObjectPool<ActorNode> ActorPool { get; private set; }


        /// <summary> A reference to the data controller. </summary>
        private DataController _dataController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _dataController = DataController.Instance;
            ActorPool = new ObjectPool<ActorNode>(_actorParent, _actorPrefab, 0);
        }


        /// <summary> Initialise a new actor. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised actor. </returns>
        public ActorNode CreateActor(String kind, Vector3 position)
        {
            ActorData data = _dataController.CreateEntityData<ActorData>(kind);
            return CreateActor(data, position);
        }


        /// <summary> Initialise a new actor. </summary>
        /// <param name="data"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised actor. </returns>
        public ActorNode CreateActor(ActorData data, Vector3 position)
        {
            ActorNode actor = ActorPool.GetAvailableObject();
            actor.Initialise(data, position);
            return actor;
        }


        /// <summary> Get a reference to the player being. </summary>
        public ActorNode GetPlayer() => _playerController.PlayerBeing;


        /// <summary> Set the current entity being controlled by the player. </summary>
        /// <param name="controllable"> The new entity to control. A null resets it back to the default being. </param>
        public void SetPlayerControllable(IControllable? controllable = null) => _playerController.SetControllable(controllable);


        /// <summary> Get the currently selected entity the player is interacting with. </summary>
        /// <returns> Either a reference to the selected entity, or a null if there isn't one. </returns>
        public IEntity? GetPlayerInteractable() => _playerController.GetCurrentInteractable();
    }
}
