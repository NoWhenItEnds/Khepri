using System;
using Godot;
using Khepri.Rooms;

namespace Khepri.Worlds.Definitions
{
    /// <summary> A designer-authored Godot resource describing a single room instance within a world definition: a unique identifier, the prefab template to build it from, and the entity placements to populate it with. </summary>
    /// <remarks> The <see cref="Id"/> field serves as the primary key used by <see cref="RoomConnection"/> to identify endpoints. Duplicate or blank ids are rejected by <see cref="WorldBuilder"/> at build time. </remarks>
    [GlobalClass]
    public partial class RoomInstance : Resource
    {
        /// <summary> The instance identifier that uniquely names this room within its <see cref="WorldDefinition"/>; used as the key in <see cref="RoomConnection"/> entries. Must be non-blank and unique across all room instances in the same world. </summary>
        [Export] public String Id { get; set; } = String.Empty;

        /// <summary> The room prefab that defines the features (description, geometry, etc.) applied when this instance is built. Must be set; a null reference causes <see cref="WorldBuilder"/> to throw at build time. </summary>
        [Export] public RoomPrefab? Prefab { get; set; }

        /// <summary> The entity placements that are spawned into this room during the world-building pass. May be empty when the room starts without any entities. </summary>
        [Export] public Godot.Collections.Array<EntityPlacement> Entities { get; set; } = new Godot.Collections.Array<EntityPlacement>();
    }
}
