using System;
using Godot;
using Khepri.Nodes.Singletons;
using Khepri.Resources;
using Khepri.Resources.Devices;

namespace Khepri.Entities.Devices
{
    /// <summary> A factory for making device objects. </summary>
    public partial class DeviceController : SingletonNode3D<DeviceController>
    {
        /// <summary> A reference to the resource controller. </summary>
        private ResourceController _resourceController;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _resourceController = ResourceController.Instance;
        }


        /// <summary> Initialise a new device. </summary>
        /// <param name="kind"> The specific kind or common name of the resource. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item, or a null if one couldn't be created. </returns>
        public DeviceNode? CreateDevice(String kind, Vector3 position)
        {
            DeviceResource? resource = _resourceController.CreateResource<DeviceResource>(kind);
            if (resource == null)
            {
                return null;
            }
            return CreateDevice(resource, position);
        }


        /// <summary> Initialise a new device. </summary>
        /// <param name="resource"> The data resource to associate with this node. </param>
        /// <param name="position"> The position to create the object at. </param>
        /// <returns> The initialised item. </returns>
        public DeviceNode CreateDevice(DeviceResource resource, Vector3 position)
        {
            throw new NotImplementedException();
        }
    }
}
