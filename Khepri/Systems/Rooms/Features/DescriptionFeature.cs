using System;
using Khepri.Prefabs;
using Khepri.Rooms.Prefabs;

namespace Khepri.Rooms.Features
{
    /// <summary> A feature that supplies a fixed textual description for a room, backing <see cref="Room.BuildDescription"/>. </summary>
    public sealed class DescriptionFeature : Feature
    {
        /// <summary> The description text returned to callers of <see cref="Room.BuildDescription"/>. </summary>
        public readonly String Text;


        /// <summary> Initialises a new description feature attached to the given room. </summary>
        /// <param name="room"> The room that this feature belongs to. </param>
        /// <param name="text"> The description text to expose. Must not be null or whitespace. </param>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="text"/> is null or whitespace. </exception>
        public DescriptionFeature(Room room, String text) : base(room)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Description text must not be null or whitespace.", nameof(text));
            }

            Text = text;
        }


        /// <summary> Creates a <see cref="DescriptionFeature"/> from prefab data, reading the required <c>"text"</c> property. </summary>
        /// <param name="room"> The room the feature will be attached to. </param>
        /// <param name="data"> The feature's parsed JSON data. </param>
        /// <returns> A fully constructed <see cref="DescriptionFeature"/>. </returns>
        [FeatureFactory]
        private static DescriptionFeature Create(Room room, PrefabData data)
        {
            return new DescriptionFeature(room, data.GetString("text"));
        }
    }
}
