using Khepri.Entities.Actors;
using System;

namespace Khepri.Data.Items
{
    /// <summary> The data component for an item that can be wielded. </summary>
    public class ToolData : ItemData
    {
        /// <inheritdoc/>
        public override void Use(ActorNode activatingEntity)
        {
            throw new NotImplementedException();
        }
    }
}
