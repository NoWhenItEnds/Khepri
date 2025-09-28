using Godot;
using Khepri.Entities.Items;
using System.Collections.Generic;

namespace Khepri.Entities.Components
{
    /// <summary> A data structure representing an entity's inventory. </summary>
    public partial class EntityInventory : Node
    {
        /// <summary> The items being stored in the entity's inventory. </summary>
        public List<IItem> StoredItems { get; private set; }


        public void AddItem(IItem item)
        {
            StoredItems.Add(item);
        }
    }
}
