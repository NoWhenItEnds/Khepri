using System;
using Godot;
using Khepri.Rooms;

namespace Khepri.Worlds.Definitions
{
    /// <summary> A designer-authored Godot resource that declares a bidirectional connection between two room instances within a <see cref="WorldDefinition"/>, identified by their <see cref="RoomInstance.Id"/> values and the position within each room at which the connection attaches. </summary>
    /// <remarks>
    /// Named <c>RoomConnection</c> rather than <c>Connection</c> to avoid a name collision with <see cref="Connection"/>, which is the live runtime link class.
    /// The two positions are independent (non-Euclidean) — no geometric relationship between them is assumed or enforced.
    /// <see cref="ToId"/> may equal <see cref="FromId"/> provided <see cref="ToPosition"/> differs from <see cref="FromPosition"/> — this represents a self-connection (a room joining two of its own slots).
    /// </remarks>
    [GlobalClass]
    public partial class RoomConnection : Resource
    {
        /// <summary> The <see cref="RoomInstance.Id"/> of the first room endpoint. Must match a declared id in the containing <see cref="WorldDefinition.Rooms"/> list. </summary>
        [Export] public String FromId { get; set; } = String.Empty;

        /// <summary> The position within the room identified by <see cref="FromId"/> at which this connection attaches. Displayed as a dropdown in the Godot inspector; the two positions are independent. </summary>
        [Export] public RoomPosition FromPosition { get; set; } = RoomPosition.Center;

        /// <summary> The <see cref="RoomInstance.Id"/> of the second room endpoint. Must match a declared id in the containing <see cref="WorldDefinition.Rooms"/> list. May equal <see cref="FromId"/> when <see cref="ToPosition"/> differs — this declares a self-connection. </summary>
        [Export] public String ToId { get; set; } = String.Empty;

        /// <summary> The position within the room identified by <see cref="ToId"/> at which this connection attaches. Displayed as a dropdown in the Godot inspector; the two positions are independent. </summary>
        [Export] public RoomPosition ToPosition { get; set; } = RoomPosition.Center;

        /// <summary> Travel cost through this connection in metres; used by pathfinding. Defaults to 0. </summary>
        [Export] public Single Distance { get; set; } = 0f;
    }
}
