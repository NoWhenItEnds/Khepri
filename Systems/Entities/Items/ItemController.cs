using System;
using Godot;
using Godot.Collections;
using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Items;
using Khepri.Types;
using Khepri.Types.Exceptions;

namespace Khepri.Entities.Items
{
    /// <summary> A factory for making item objects. </summary>
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
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public ItemNode CreateItem(ItemResource resource, Vector3 position)
        {
            ItemNode item = ItemPool.GetAvailableObject();
            item.Initialise(resource, position);
            return item;
        }


        /// <inheritdoc/>
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key && key.IsReleased())
            {
                if (key.Keycode == Key.Ctrl)
                {
                    GD.Print(OS.GetDataDir());
                    Save();
                }
                else if (key.Keycode == Key.Alt)
                {
                    Load("user://save_game.dat");
                }
            }
        }



        public void Save()
        {
            FileAccess? file = FileAccess.Open("user://save_game.dat", FileAccess.ModeFlags.Write);
            if (file == null)
            {
                throw new ItemException(FileAccess.GetOpenError().ToString());
            }

            ItemNode[] activeObjects = ItemPool.GetActiveObjects();
            foreach (ItemNode item in activeObjects)
            {
                Dictionary<String, Variant> data = item.Serialise();
                file.StoreVar(data);
            }
            file.Close();
        }


        public void Load(String filepath)
        {
            FileAccess? file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                throw new ItemException(FileAccess.GetOpenError().ToString());
            }

            Variant t = file.GetVar();
            GD.Print(t);
        }
    }
}
