using System;
using Godot;

namespace Khepri.Entities
{
    /// <summary> A data-bearing resource that gives a part its specific identity. </summary>
    [GlobalClass]
    public partial class EntityKind : Resource
    {
        /// <summary> The noun this kind claims for any part that references it, for example <c>"goblin"</c> or <c>"chest"</c>. </summary>
        [Export] public String Noun { get; set; } = String.Empty;
    }
}
