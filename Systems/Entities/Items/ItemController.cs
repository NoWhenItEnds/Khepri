using System;
using Godot;
using Khepri.Data;
using Khepri.Data.Items;
using Khepri.Nodes.Singletons;
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


        /// <summary> A reference to the data controller. </summary>
        private DataController _dataController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _dataController = DataController.Instance;
            ItemPool = new ObjectPool<ItemNode>(this, _itemPrefab);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public ItemNode CreateItem(String kind, Vector3 position)
        {
            ItemData resource = _dataController.CreateEntityData<ItemData>(kind);
            return CreateItem(resource, position);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public ItemNode CreateItem(ItemData resource, Vector3 position)
        {
            ItemNode item = ItemPool.GetAvailableObject();
            item.Initialise(resource, position);
            return item;
        }
    }
}
