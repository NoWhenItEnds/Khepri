using Godot;
using Godot.Collections;
using Khepri.Entities.Actors;
using System;

namespace Khepri.Resources.Items
{
    /// <summary> The data component for an item that can be wielded. </summary>
    [GlobalClass]
    public partial class ToolResource : ItemResource
    {
        /// <summary> The data component for an item that can be wielded. </summary>
        public ToolResource() { }


        /// <inheritdoc/>
        public override void Use(ActorNode activatingEntity)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override void Deserialise(Dictionary<String, Variant> data)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public override Dictionary<String, Variant> Serialise()
        {
            throw new NotImplementedException();
        }
    }
}
