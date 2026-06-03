using System;
using System.Collections.Generic;

namespace Khepri.Rooms
{
    /// <summary> An immutable, parsed representation of a world definition file: a set of room instance specs and the connections between them. </summary>
    /// <remarks> This is a pure data structure — it holds no <see cref="Room"/> instances and performs no construction. The actual world-building step (instantiating rooms, spawning entities, wiring connections) is handled by a separate Godot-layer manager. </remarks>
    /// <param name="Rooms"> The ordered list of room instance specs declared in the world definition. </param>
    /// <param name="Connections"> The list of connection specs declaring which room instances are linked. </param>
    public sealed record WorldDefinition(
        IReadOnlyList<RoomInstanceSpec> Rooms,
        IReadOnlyList<ConnectionSpec>   Connections);
}
