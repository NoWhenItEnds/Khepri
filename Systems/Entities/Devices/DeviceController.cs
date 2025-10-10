using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Devices;
using Khepri.Types;

namespace Khepri.Entities.Devices
{
    /// <summary> A factory for making device objects. </summary>
    public partial class DeviceController : SingletonNode3D<DeviceController>
    {
        /// <summary> A pool of instantiated items to pull from first. </summary>
        public ObjectPool<DeviceNode, DeviceResource> ItemPool { get; private set; }


        /// <summary> A reference to the resource controller. </summary>
        private ResourceController _resourceController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _resourceController = ResourceController.Instance;
            //ItemPool = new ObjectPool<DeviceNode, DeviceResource>(this, _itemPrefab);
        }
    }
}
