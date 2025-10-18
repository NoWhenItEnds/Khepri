using Aphelion.Types.Extensions;
using Godot;
using Godot.Collections;
using Khepri.Entities.Actors;
using Khepri.Entities.Devices;
using Khepri.Entities.Items;
using Khepri.Nodes.Singletons;
using Khepri.Types.Exceptions;
using System;

namespace Khepri.Controllers
{
    /// <summary> Controls the world's state such as its time and seasons. </summary>
    public partial class WorldController : SingletonNode<WorldController>
    {
        /// <summary> The observer's latitude. </summary>
        [ExportGroup("Settings")]
        [Export] public Double Latitude { get; private set; } = -31.80529;

        /// <summary> The observer's longitude. </summary>
        [Export] public Double Longitude { get; private set; } = 115.74419;

        /// <summary> How quickly time is moving. </summary>
        /// <remarks> One in game day takes two hours. </remarks>
        [Export] private Single _timescale = 12f;

        /// <summary> How many degrees each pfile of star in the array represents. </summary>
        [Export] private Single _starSegmentSize = 2f;


        /// <summary> The world's current time. </summary>
        /// <remarks> The game starts on Sunday the 5th of February, 2012. </remarks>
        public DateTimeOffset CurrentTime { get; private set; } = new DateTimeOffset(2012, 2, 5, 0, 0, 0, TimeSpan.FromHours(8));

        /// <summary> The current local sidereal time relative to the observer's longitude. </summary>
        public Double LocalSiderealTime => CurrentTime.ToLocalSiderealTime(Longitude);

        /// <summary> The amount of time (in seconds) that has passed in game time since the previous physics frame. </summary>
        public Double GameTimeDelta { get; private set; }


        /// <summary> Announce that it is the dawn of a new day. </summary>
        public event Action<DateOnly> NewDay;


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            DateTimeOffset previousTime = CurrentTime;
            CurrentTime = CurrentTime.AddSeconds(delta * _timescale);
            GameTimeDelta = (CurrentTime.ToUnixTimeMilliseconds() - previousTime.ToUnixTimeMilliseconds()) * 0.001f;

            // If the day has changed, fire the new day event.
            if (CurrentTime.Day != previousTime.Day)
            {
                NewDay?.Invoke(DateOnly.FromDateTime(CurrentTime.Date));
            }
        }


        /// <summary> Save all the entities within the world to the persistent data. </summary>
        /// <exception cref="System.IO.FileLoadException"> If a file is unable to be opened. </exception>
        public void Save()
        {
            FileAccess? file = FileAccess.Open("user://save_game.dat", FileAccess.ModeFlags.Write);
            if (file == null)
            {
                throw new System.IO.FileLoadException(FileAccess.GetOpenError().ToString());
            }

            Array<Dictionary<String, Variant>> data = new Array<Dictionary<string, Variant>>();
            data.AddRange(ActorController.Instance.Serialise());
            data.AddRange(DeviceController.Instance.Serialise());
            data.AddRange(ItemController.Instance.Serialise());
            file.StoreVar(data);
            file.Close();
        }


        /// <summary> Load all the entities within the world from the persistent data. </summary>
        /// <param name="filepath"> The filepath of the save data to load. </param>
        /// <exception cref="System.IO.FileLoadException"> If a file or item is unable to be loaded. </exception>
        public void Load(String filepath)
        {
            FileAccess? file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                throw new System.IO.FileLoadException(FileAccess.GetOpenError().ToString());
            }

            Array<Dictionary<String, Variant>> data = (Array<Dictionary<String, Variant>>)file.GetVar();
            Dictionary<String, Array<Dictionary<String, Variant>>> filteredEntities = FilterEntities(data);

            if (filteredEntities.TryGetValue("actor", out Array<Dictionary<String, Variant>>? actors) && actors != null)
            {
                ActorController.Instance.Deserialise(actors);
            }
            if (filteredEntities.TryGetValue("device", out Array<Dictionary<String, Variant>>? devices) && devices != null)
            {
                DeviceController.Instance.Deserialise(devices);
            }
            if (filteredEntities.TryGetValue("item", out Array<Dictionary<String, Variant>>? items) && items != null)
            {
                ItemController.Instance.Deserialise(items);
            }
            if (filteredEntities.TryGetValue("celestial", out Array<Dictionary<String, Variant>>? celestials) && actors != null)
            {
                // TODO - Pass to celestial controller.
            }
        }


        /// <summary> Filter the unstructured data into categories of types, so that they can be given to the correct controller. </summary>
        /// <param name="data"> The unstructured data. </param>
        /// <returns> The data filtered by the resource type. </returns>
        /// <exception cref="ResourceException"> If a resource hasn't been formatted correctly. </exception>
        private Dictionary<String, Array<Dictionary<String, Variant>>> FilterEntities(Array<Dictionary<String, Variant>> data)
        {
            Dictionary<String, Array<Dictionary<String, Variant>>> results = new Dictionary<String, Array<Dictionary<String, Variant>>>()
            {
                { "actor", new Array<Dictionary<String, Variant>>() },
                { "device", new Array<Dictionary<String, Variant>>() },
                { "item", new Array<Dictionary<String, Variant>>() },
                { "celestial", new Array<Dictionary<String, Variant>>() }
            };

            foreach (Dictionary<String, Variant> item in data)
            {
                if (item.TryGetValue("id", out Variant id))
                {
                    // The kind of a resource should be the first value in its identifier.
                    String kind = ((String)id).Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

                    if (results.TryGetValue(kind, out Array<Dictionary<String, Variant>>? key) && key != null)
                    {
                        key.Add(item);
                    }
                }
                else
                {
                    throw new ResourceException("Every resource needs an 'id' field to help identify its type. I just tried to load one without one.");
                }
            }

            return results;
        }
    }
}
