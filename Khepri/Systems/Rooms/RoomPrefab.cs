using System;
using Godot;
using Khepri.Rooms.Features;

namespace Khepri.Rooms
{
    /// <summary> A reusable, designer-authored template describing the feature set of a room, saved as a <c>.tres</c> resource and instantiated into a live <see cref="Room"/> on demand. </summary>
    /// <remarks> Holds template <see cref="Feature"/> resources directly; instantiation duplicates each so every room gets its own features, then binds the owner and calls <see cref="Feature.OnInstantiate"/>. </remarks>
    [GlobalClass]
    public partial class RoomPrefab : Resource
    {
        /// <summary> The name this prefab is registered and looked up under (for example <c>"stone_chamber"</c>). Must be unique across all loaded room prefabs. </summary>
        [Export] public String Name { get; set; } = String.Empty;

        /// <summary> The template features applied to each room built from this prefab. Each is duplicated per room at instantiation. </summary>
        [Export] public Godot.Collections.Array<Feature> Features { get; set; } = new();


        /// <summary> Builds a fresh <see cref="Room"/> with a new identity and a private copy of each template feature. </summary>
        /// <returns> A fully constructed room. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when two templates resolve to the same feature type, or propagated from <see cref="Feature.OnInstantiate"/>. </exception>
        public Room Instantiate()
        {
            Room room = new Room(Guid.NewGuid());

            foreach (Feature template in Features)
            {
                Feature instance = (Feature)template.Duplicate(false);
                instance.Initialise(room);
                instance.OnInstantiate();

                Boolean added = room.AddComponent(instance);

                if (!added)
                {
                    throw new InvalidOperationException(
                        $"Room prefab '{Name}' declares two features that resolve to the same type ('{instance.GetType().Name}').");
                }
            }

            return room;
        }
    }
}
