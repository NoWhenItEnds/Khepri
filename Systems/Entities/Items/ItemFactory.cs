using Godot;
using Khepri.Entities.Items.Components;
using Khepri.Nodes.Singletons;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Items
{
    /// <summary> A factory for making item objects. </summary>
    public partial class ItemFactory : SingletonNode3D<ItemFactory>
    {
        /// <summary> The prefab to use for creating items. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _itemPrefab;


        /// <summary> How many item nodes the item pool should contain. </summary>
        [ExportGroup("Settings")]
        [Export] private Int32 _poolSize = 100;


        /// <summary> A pool that holds items that are in the game world. A boolean shows if an object is currently in use. </summary>
        private Dictionary<Item, Boolean> _itemPool = new Dictionary<Item, Boolean>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            // First look for any existing items.
            foreach (Node node in GetChildren())
            {
                if (node is Item item)
                {
                    _itemPool.Add(item, true);
                }
            }

            // Build the pool.
            for (Int32 i = 0; i < _poolSize - _itemPool.Count; i++)
            {
                Item item = BuildItem();
                _itemPool.Add(item, false);
            }

            TempSpawn();
        }


        private void TempSpawn()
        {
            ItemDataComponent data = new ItemDataComponent([new Vector2I(0, 0), new Vector2I(1, 0)]);
            CreateItem(data, new Vector3(3, 0, -3));
        }


        /// <summary> Create a new item for the pool. </summary>
        /// <returns> The created item node. </returns>
        private Item BuildItem()
        {
            Item item = _itemPrefab.Instantiate<Item>();
            AddChild(item);
            item.GlobalPosition = new Vector3(0f, -10000f, 0f);
            return item;
        }


        /// <summary> Initialise a new item by pulling from the pool. </summary>
        /// <param name="data"> The data to initialise the item with. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialise item. </returns>
        public Item CreateItem(ItemDataComponent data, Vector3 position)
        {
            Item result = null;

            // Attempt to find a free item in the pool.
            foreach (KeyValuePair<Item, Boolean> item in _itemPool)
            {
                if (!item.Value)    // If the item is free.
                {
                    result = item.Key;
                    break;
                }
            }

            // If an item was not found, expand the pool.
            if (result == null)
            {
                result = BuildItem();
                _poolSize++;
            }

            _itemPool[result] = true;
            result.Initialise(data);
            result.GlobalPosition = position;

            return result;
        }


        /// <summary> Free an item and return it to the pool. </summary>
        /// <param name="item"> The item to return. </param>
        public void FreeItem(Item item)
        {
            _itemPool[item] = false;
            item.GlobalPosition = new Vector3(0f, -10000f, 0f);
        }
    }
}
