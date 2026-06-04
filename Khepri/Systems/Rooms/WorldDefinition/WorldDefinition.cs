using System;
using Godot;

namespace Khepri.Rooms
{
    /// <summary> A designer-authored Godot resource that declares the full set of rooms and connections that constitute a world, saved as a <c>.tres</c> file and loaded at runtime via <see cref="ResourceLoader"/>. </summary>
    /// <remarks> This resource replaces the JSON world definition layer. Rooms hold direct <see cref="RoomInstance"/> references that each carry a <see cref="RoomPrefab"/> and a list of <see cref="EntityPlacement"/> entries — no name-based lookup is required. </remarks>
    [GlobalClass]
    public partial class WorldDefinition : Resource
    {
        /// <summary> The ordered list of room instances that make up this world. Must contain at least one entry for the world to be valid. </summary>
        [Export] public Godot.Collections.Array<RoomInstance> Rooms { get; set; } = new Godot.Collections.Array<RoomInstance>();

        /// <summary> The bidirectional connections linking pairs of rooms declared in <see cref="Rooms"/>. May be empty when the world has no traversable exits. </summary>
        [Export] public Godot.Collections.Array<RoomConnection> Connections { get; set; } = new Godot.Collections.Array<RoomConnection>();
    }
}
