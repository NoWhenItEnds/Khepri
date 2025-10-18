using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Devices;
using Khepri.Types;

namespace Khepri.Entities.Devices
{
    /// <summary> A factory for making device objects. </summary>
    public partial class DeviceController : SingletonNode3D<DeviceController>
    {
        /// <summary> The prefab to use for creating devices. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _itemPrefab;


        /// <summary> A pool of instantiated devices to pull from first. </summary>
        public ObjectPool<DeviceNode> DevicePool { get; private set; }


        /// <summary> A reference to the resource controller. </summary>
        private ResourceController _resourceController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _resourceController = ResourceController.Instance;
            DevicePool = new ObjectPool<DeviceNode>(this, _itemPrefab);
        }


        /// <summary> Initialise a new device. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised device. </returns>
        public DeviceNode CreateDevice(String kind, Vector3 position)
        {
            DeviceResource resource = _resourceController.CreateResource<DeviceResource>(kind);
            return CreateDevice(resource, position);
        }


        /// <summary> Initialise a new device. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public DeviceNode CreateDevice(DeviceResource resource, Vector3 position)
        {
            DeviceNode device = DevicePool.GetAvailableObject();
            device.Initialise(resource, position);
            return device;
        }


        /// <summary> Package the world state for saving. </summary>
        /// <returns> An array of the devices representing the current world state. </returns>
        public Array<Dictionary<String, Variant>> Serialise()
        {
            DeviceNode[] activeObjects = DevicePool.GetActiveObjects();
            Array<Dictionary<String, Variant>> data = new Array<Dictionary<String, Variant>>();
            foreach (DeviceNode item in activeObjects)
            {
                data.Add(item.Serialise());
            }
            return data;
        }


        /// <summary> Unpack the given data and instantiate the world state. </summary>
        /// <param name="data"> Data that has the 'device' type to unpack. </param>
        public void Deserialise(Array<Dictionary<String, Variant>> data)
        {
            DeviceNode[] activeObjects = DevicePool.GetActiveObjects();

            foreach (Dictionary<String, Variant> item in data)
            {
                UInt64 uid = (UInt64)item["uid"];
                String id = (String)item["id"];
                Vector3 position = (Vector3)item["position"];

                DeviceNode? newDevice = activeObjects.FirstOrDefault(x => x.UId == uid) ?? null;
                if (newDevice == null)
                {
                    newDevice = CreateDevice(id, position);
                }

                newDevice.Deserialise(item);
            }
        }
    }
}
