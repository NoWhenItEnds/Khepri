using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a nano forge device. </summary>
    [GlobalClass]
    public partial class NanoForgeResource : DeviceResource
    {
        /// <summary> The data component of a telescope device. </summary>
        public NanoForgeResource() {}


        /// <inheritdoc/>
        public override void Use(Being activatingBeing)
        {
            ItemController.Instance.CreateItem("apple", activatingBeing.GlobalPosition);
        }
    }
}
