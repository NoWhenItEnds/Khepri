using Godot;
using Khepri.Models.Persistent;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A type of entity representing something that can be grabbed and placed in an inventory. </summary>
    public interface IItem
    {
        /// <summary> The items internal data model. </summary>
        public ItemData Data { get; }
    }
}
