using System;
using Godot;
using Khepri.Data;
using Khepri.Data.Devices;
using Khepri.Nodes.Singletons;
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


        /// <summary> A reference to the data controller. </summary>
        private DataController _dataController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _dataController = DataController.Instance;
            DevicePool = new ObjectPool<DeviceNode>(this, _itemPrefab);
        }


        /// <summary> Initialise a new device. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised device. </returns>
        public DeviceNode CreateDevice(String kind, Vector3 position)
        {
            DeviceData data = _dataController.CreateEntityData<DeviceData>(kind);
            return CreateDevice(data, position);
        }


        /// <summary> Initialise a new device. </summary>
        /// <param name="data"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public DeviceNode CreateDevice(DeviceData data, Vector3 position)
        {
            DeviceNode device = DevicePool.GetAvailableObject();
            device.Initialise(data, position);
            return device;
        }
    }
}
