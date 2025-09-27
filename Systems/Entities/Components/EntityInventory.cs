using Khepri.Entities.Items;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Khepri.Entities.Components
{
    /// <summary> A data structure representing an entity's inventory. </summary>
    public record EntityInventory
    {
        /// <summary> The items being stored in the entity's inventory. </summary>
        [JsonPropertyName("stored_items")]
        public List<IItem> StoredItems { get; private set; }
    }
}
