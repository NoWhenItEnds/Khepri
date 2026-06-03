using System;
using Khepri.Prefabs;
using Khepri.Rooms.Features;

namespace Khepri.Rooms
{
    /// <summary> Entry point for constructing new rooms, either bare or from a room prefab template. </summary>
    /// <remarks> Godot-free and has no dependency on EntityManager or any entity-spawning infrastructure. Room-to-room connections and entity placement are performed by a separate manager in the Godot layer. </remarks>
    public static class RoomFactory
    {
        /// <summary> Creates a new room with a freshly generated unique identifier. </summary>
        /// <returns> A new <see cref="Room"/> with a generated <see cref="Guid"/>. </returns>
        public static Room Create()
        {
            return new Room(GenerateUId());
        }


        /// <summary> Creates a new room and applies all feature factories defined on <paramref name="prefab"/> in registration order. </summary>
        /// <remarks> Because the prefab's feature set is applied at creation time, attaching a feature of the same type afterwards will trigger the duplicate guard and throw <see cref="InvalidOperationException"/>. Each feature receives a reference to the newly created room at construction time, exactly as components receive their parent entity. </remarks>
        /// <param name="prefab"> The prefab whose feature set to apply to the new room. </param>
        /// <returns> A fully assembled <see cref="Room"/> with all prefab features attached. </returns>
        /// <exception cref="InvalidOperationException"> Propagated from the prefab builder when a feature factory returns <c>null</c> or registers a duplicate feature type. </exception>
        public static Room CreateFrom(Prefab<Room, Feature> prefab)
        {
            Room room = Create();
            PrefabBuilder<Room, Feature> builder = new PrefabBuilder<Room, Feature>(room);
            prefab.ApplyTo(builder);
            return room;
        }


        /// <summary> Generates a new unique identifier for a room, keeping all ID generation in one place. </summary>
        /// <returns> A newly generated <see cref="Guid"/>. </returns>
        private static Guid GenerateUId()
        {
            return Guid.NewGuid();
        }
    }
}
