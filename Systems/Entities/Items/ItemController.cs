using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Items;
using Khepri.Types;

namespace Khepri.Entities.Items
{
    /// <summary> A controller for item entities within the game world. </summary>
    public partial class ItemController : SingletonNode3D<ItemController>
    {
        /// <summary> The prefab to use for creating items. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _itemPrefab;


        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<ItemNode> ItemPool { get; private set; }


        /// <summary> A reference to the resource controller. </summary>
        private ResourceController _resourceController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _resourceController = ResourceController.Instance;
            ItemPool = new ObjectPool<ItemNode>(this, _itemPrefab);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public ItemNode CreateItem(String kind, Vector3 position)
        {
            ItemResource resource = _resourceController.CreateResource<ItemResource>(kind);
            return CreateItem(resource, position);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public ItemNode CreateItem(ItemResource resource, Vector3 position)
        {
            ItemNode item = ItemPool.GetAvailableObject();
            item.Initialise(resource, position);
            return item;
        }


        /// <summary> Package the world state for saving. </summary>
        /// <returns> An array of the items representing the current world state. </returns>
        public Array<Dictionary<String, Variant>> Serialise()
        {
            ItemNode[] activeObjects = ItemPool.GetActiveObjects();
            Array<Dictionary<String, Variant>> data = new Array<Dictionary<String, Variant>>();
            foreach (ItemNode item in activeObjects)
            {
                data.Add(item.Serialise());
            }
            return data;
        }


        /// <summary> Unpack the given data and instantiate the world state. </summary>
        /// <param name="data"> Data that has the 'item' type to unpack. </param>
        public void Deserialise(Array<Dictionary<String, Variant>> data)
        {
            ItemNode[] activeObjects = ItemPool.GetActiveObjects();

            foreach (Dictionary<String, Variant> item in data)
            {
                UInt64 uid = (UInt64)item["uid"];
                String id = (String)item["id"];
                Vector3 position = (Vector3)item["position"];

                ItemNode? newItem = activeObjects.FirstOrDefault(x => x.UId == uid) ?? null;
                if (newItem == null)
                {
                    newItem = CreateItem(id, position);
                }

                newItem.Deserialise(item);
            }
        }
    }
}
