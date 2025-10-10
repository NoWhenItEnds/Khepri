using System;
using Godot;

namespace Khepri.Resources
{
    /// <summary> The data component of a game entity. </summary>
    public abstract partial class EntityResource : Resource
    {
        /// <summary> The resource's identifying kind. </summary>
        [ExportGroup("General")]
        [Export] public String Id { get; private set; }


        /// <summary> The data component of a game entity. </summary>
        public EntityResource() { }
    }
}
