using System;

namespace Khepri.Rooms
{
    /// <summary> An immutable specification for a single bidirectional connection between two room instances within a world definition. </summary>
    /// <param name="FromId"> The instance identifier of the first room endpoint; must match a <see cref="RoomInstanceSpec.Id"/> declared in the same world definition. </param>
    /// <param name="ToId"> The instance identifier of the second room endpoint; must match a <see cref="RoomInstanceSpec.Id"/> declared in the same world definition. </param>
    public sealed record ConnectionSpec(String FromId, String ToId);
}
