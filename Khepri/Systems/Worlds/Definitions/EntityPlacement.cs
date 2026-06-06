using Godot;
using Khepri.Entities;
using Khepri.Rooms;

namespace Khepri.Worlds.Definitions
{
    /// <summary> A designer-authored Godot resource that pairs an entity prefab with the <see cref="RoomPosition"/> slot it should occupy when spawned into a room. </summary>
    /// <remarks> This resource replaces the bare entity-prefab-name strings from the JSON layer, adding a mandatory position so every placement unambiguously declares where in the room the entity starts. </remarks>
    [GlobalClass]
    public partial class EntityPlacement : Resource
    {
        /// <summary> The entity prefab to instantiate for this placement. Must be set; a null reference causes <see cref="WorldBuilder"/> to throw at build time with the owning room id as context. </summary>
        [Export] public EntityPrefab? Prefab { get; set; }

        /// <summary> The position slot within the room that the spawned entity occupies. Displayed as a dropdown in the Godot inspector. </summary>
        [Export] public RoomPosition Position { get; set; } = RoomPosition.Center;
    }
}
