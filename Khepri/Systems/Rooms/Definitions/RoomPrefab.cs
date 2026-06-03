using System;
using Godot;
using Khepri.Rooms.Features;

namespace Khepri.Rooms.Definitions
{
    /// <summary> A reusable, designer-authored template describing the feature set of a room, saved as a <c>.tres</c> resource and instantiated into a live <see cref="Room"/> on demand. </summary>
    /// <remarks> The room-side counterpart to <c>EntityPrefab</c>; replaces the former JSON-prefab + feature-registry stack. </remarks>
    [GlobalClass]
    public partial class RoomPrefab : Resource
    {
        /// <summary> The name this prefab is registered and looked up under (for example <c>"stone_chamber"</c>). Must be unique across all loaded room prefabs. </summary>
        [Export] public String Name { get; set; } = String.Empty;

        /// <summary> The ordered feature definitions applied to each room built from this prefab. </summary>
        [Export] public Godot.Collections.Array<FeatureDef> Features { get; set; } = new();


        /// <summary> Builds a fresh <see cref="Room"/> with a new identity and a feature for each definition in <see cref="Features"/>. </summary>
        /// <returns> A fully constructed room. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when two definitions produce features of the same type (a room holds at most one feature per type). </exception>
        public Room Instantiate()
        {
            Room room = new Room(Guid.NewGuid());

            foreach (FeatureDef definition in Features)
            {
                Feature feature = definition.Create(room);
                Boolean added   = room.AddFeature(feature);

                if (!added)
                {
                    throw new InvalidOperationException(
                        $"Room prefab '{Name}' declares two features that resolve to the same type ('{feature.GetType().Name}').");
                }
            }

            return room;
        }
    }
}
