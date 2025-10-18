using Godot;
using Godot.Collections;
using Khepri.Controllers;
using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Actors;
using Khepri.Types;
using System;
using System.Linq;

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


        /// <summary> A reference to the resource controller. </summary>
        private ResourceController _resourceController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _resourceController = ResourceController.Instance;
            ActorPool = new ObjectPool<ActorNode>(_actorParent, _actorPrefab, 0);
        }


        /// <summary> Initialise a new actor. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised actor. </returns>
        public ActorNode CreateActor(String kind, Vector3 position)
        {
            ActorResource resource = _resourceController.CreateResource<ActorResource>(kind);
            return CreateActor(resource, position);
        }


        /// <summary> Initialise a new actor. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised actor. </returns>
        public ActorNode CreateActor(ActorResource resource, Vector3 position)
        {
            ActorNode actor = ActorPool.GetAvailableObject();
            actor.Initialise(resource, position);
            return actor;
        }


        /// <summary> Package the world state for saving. </summary>
        /// <returns> An array of the actors representing the current world state. </returns>
        public Array<Dictionary<String, Variant>> Serialise()
        {
            ActorNode[] activeObjects = ActorPool.GetActiveObjects();
            Array<Dictionary<String, Variant>> data = new Array<Dictionary<String, Variant>>();
            foreach (ActorNode item in activeObjects)
            {
                data.Add(item.Serialise());
            }
            return data;
        }


        /// <summary> Unpack the given data and instantiate the world state. </summary>
        /// <param name="data"> Data that has the 'actor' type to unpack. </param>
        public void Deserialise(Array<Dictionary<String, Variant>> data)
        {
            ActorNode[] activeObjects = ActorPool.GetActiveObjects();

            foreach (Dictionary<String, Variant> item in data)
            {
                UInt64 uid = (UInt64)item["uid"];
                String id = (String)item["id"];
                Vector3 position = (Vector3)item["position"];

                ActorNode? newActor = activeObjects.FirstOrDefault(x => x.UId == uid) ?? null;
                if (newActor == null)
                {
                    newActor = CreateActor(id, position);
                }

                newActor.Deserialise(item);
            }
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
