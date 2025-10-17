using System;
using Godot;
using Godot.Collections;
using Khepri.Entities.Actors;
using Khepri.Entities.Items;

namespace Khepri.Resources.Devices
{
    /// <summary> The data component of a nano forge device. </summary>
    [GlobalClass]
    public partial class NanoForgeResource : DeviceResource
    {
        /// <summary> The id of the item the nano forge will attempt to spawn. </summary>
        [ExportGroup("State")]
        [Export] public String SelectedItem { get; private set; }


        /// <summary> The data component of a telescope device. </summary>
        public NanoForgeResource() { }


        /// <inheritdoc/>
        public override void Use(ActorNode activatingBeing)
        {
            ItemController.Instance.CreateItem(SelectedItem, activatingBeing.GlobalPosition);
        }


        /// <inheritdoc/>
        public override Dictionary<String, Variant> Serialise()
        {
            return new Dictionary<String, Variant>()
            {
                { "id", Id },
                { "selected_item", SelectedItem }
            };
        }


        /// <inheritdoc/>
        public override void Deserialise(Dictionary<String, Variant> data)
        {
            SelectedItem = (String)data["selected_item"];
        }
    }
}
