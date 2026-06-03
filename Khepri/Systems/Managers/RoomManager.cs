using System;
using System.Collections.Generic;
using Jaypen.Utilities.Logging;
using Jaypen.Utilities.Singletons;
using Khepri.Entities;
using Khepri.Rooms;
using Microsoft.Extensions.Logging;

namespace Khepri.Managers
{
    /// <summary> The game world's singleton managing the game's rooms and connections between them. </summary>
    public partial class RoomManager : SingletonNode<RoomManager>
    {
        /// <summary> All the rooms that exist within the game world. </summary>
        private readonly HashSet<Room> _rooms = new HashSet<Room>();

        /// <summary> The logger instance the manager uses. </summary>
        private static readonly ILogger Logger = Log.For<RoomManager>();


        // TODO - This game world should be defined by prefabs, not randomly generated, but also support additional user creations via connections.


        /// <summary> Get the room the given entity exists within. If the entity is a child of another entity, we get the room of that entity. </summary>
        /// <param name="entity"> The entity to get the room for. </param>
        /// <returns> The room that the entity's most senor parent exists within. This should never by null as an entity should always exist within a room, somewhere, </returns>
        public Room GetCurrentRoom(Entity entity)
        {
            Room? result = null;

            foreach (Room room in _rooms)
            {
                foreach (Entity parent in room.GetEntities())   // TODO - Fix this!
                {
                    result = room;
                    break;
                }
            }

            return result != null ?
                result :
                throw new ArgumentException("The given entity doesn't exist within this plane of reality! This shouldn't be possible.");
        }
    }
}
