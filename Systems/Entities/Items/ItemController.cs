using System;
using Godot;
using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Items;
using Khepri.Types;

namespace Khepri.Entities.Items
{
    /// <summary> A factory for making item objects. </summary>
    public partial class ItemController : SingletonNode3D<ItemController>
    {
        /// <summary> The prefab to use for creating items. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _itemPrefab;


        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<ItemNode, ItemResource> ItemPool { get; private set; }


        /// <summary> A reference to the resource controller. </summary>
        private ResourceController _resourceController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _resourceController = ResourceController.Instance;
            ItemPool = new ObjectPool<ItemNode, ItemResource>(this, _itemPrefab);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item, or a null if one couldn't be created. </returns>
        public ItemNode? CreateItem(String kind, Vector3 position)
        {
            ItemResource? resource = _resourceController.CreateResource<ItemResource>(kind);
            if (resource == null)
            {
                return null;
            }
            return CreateItem(resource, position);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public ItemNode CreateItem(ItemResource resource, Vector3 position)
        {
            ItemNode item = ItemPool.GetAvailableObject();
            if (item is IPoolable<ItemResource> poolable)
            {
                poolable.Initialise(resource);
            }
            item.GlobalPosition = position;
            return item;
        }
    }
}
