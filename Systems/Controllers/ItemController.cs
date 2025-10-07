using Godot;
using Khepri.Entities.Items;
using Khepri.Nodes.Singletons;
using Khepri.Types;
using System;

namespace Khepri.Controllers
{
    /// <summary> A factory for making item objects. </summary>
    public partial class ItemController : SingletonNode3D<ItemController>
    {
        /// <summary> The prefab to use for creating items. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _itemPrefab;


        /// <summary> A pool of instantiated items to pull from first. </summary>
        private ObjectPool<Item> _itemPool;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _itemPool = new ObjectPool<Item>(this, _itemPrefab);
            TempSpawn();
        }


        private void TempSpawn()
        {
            Item item0 = CreateItem("apple", ItemType.FOOD, new Vector3(3, 0, -3));
            Item item1 = CreateItem("apple", ItemType.FOOD, new Vector3(3, 0, -4));
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="itemName"> The common name of the item to spawn. </param>
        /// <param name="itemType"> The kind of item it is. Allows it to be searched. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public Item CreateItem(String itemName, ItemType itemType, Vector3 position)
        {
            ItemData data = ItemFactory.Create(itemName, itemType);
            return CreateItem(data, position);
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public Item CreateItem(ItemData data, Vector3 position)
        {
            Item item = _itemPool.CreateObject();
            item.Initialise(data, position);
            return item;
        }


        /// <summary> Dispose of an item, returning it back to the item pool. </summary>
        /// <param name="item"> The item to dispose. </param>
        public void RemoveItem(Item item) => _itemPool.FreeItem(item);
    }
}
