using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Khepri.Rooms
{
    /// <summary> Parses a world definition JSON file into a <see cref="WorldDefinition"/>, applying fail-explicit validation without constructing any rooms, spawning any entities, or wiring any connections. </summary>
    /// <remarks>
    /// <para>
    /// The expected JSON schema is:
    /// <code>
    /// {
    ///   "rooms": [
    ///     { "id": "entry_hall", "prefab": "stone_chamber", "entities": ["goblin"] },
    ///     { "id": "armoury",    "prefab": "stone_chamber" }
    ///   ],
    ///   "connections": [
    ///     { "from": "entry_hall", "to": "armoury" }
    ///   ]
    /// }
    /// </code>
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>rooms</c> — required non-empty array; each entry must have a non-blank <c>id</c>, a non-blank <c>prefab</c>, and an optional <c>entities</c> string array.</description></item>
    ///   <item><description>Room <c>id</c> values must be unique within the file.</description></item>
    ///   <item><description><c>connections</c> — optional array; each entry must have non-blank <c>from</c> and <c>to</c> fields referencing declared room ids.</description></item>
    ///   <item><description>A connection may not reference the same id for both endpoints (self-connection).</description></item>
    /// </list>
    /// </remarks>
    public sealed class WorldDefinitionLoader
    {
        /// <summary> Parses the JSON content at <paramref name="filePath"/> and returns a fully validated <see cref="WorldDefinition"/>. </summary>
        /// <param name="filePath"> The absolute or relative path to the world definition JSON file. </param>
        /// <returns> A <see cref="WorldDefinition"/> containing all room instance specs and connection specs declared in the file. </returns>
        /// <exception cref="IOException"> Thrown when the file cannot be read, wrapped with <paramref name="filePath"/> as context. </exception>
        /// <exception cref="JsonException"> Thrown when the file content is not valid JSON, wrapped with <paramref name="filePath"/> as context. </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when:
        /// <list type="bullet">
        ///   <item><description>the <c>rooms</c> array is missing or empty;</description></item>
        ///   <item><description>a room entry has a missing or blank <c>id</c> or <c>prefab</c>;</description></item>
        ///   <item><description>two room entries share the same <c>id</c>;</description></item>
        ///   <item><description>a connection references an undeclared room id;</description></item>
        ///   <item><description>a connection specifies the same id for both endpoints.</description></item>
        /// </list>
        /// </exception>
        public WorldDefinition Load(String filePath)
        {
            String       json     = ReadFile(filePath);
            JsonDocument document = ParseJson(json, filePath);

            using (document)
            {
                JsonElement root = document.RootElement;

                IReadOnlyList<RoomInstanceSpec> rooms       = ReadRooms(root, filePath);
                HashSet<String>                 knownIds    = BuildIdSet(rooms);
                IReadOnlyList<ConnectionSpec>   connections = ReadConnections(root, filePath, knownIds);

                return new WorldDefinition(rooms, connections);
            }
        }


        /// <summary> Reads the raw text content of the file, wrapping any IO or permission failure with <paramref name="filePath"/> as context. </summary>
        /// <param name="filePath"> The path to read. </param>
        /// <returns> The file's text content. </returns>
        /// <exception cref="IOException"> Thrown with <paramref name="filePath"/> as context when the file cannot be read. </exception>
        private static String ReadFile(String filePath)
        {
            String content;

            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new IOException($"World definition file '{filePath}' could not be read: {ex.Message}", ex);
            }

            return content;
        }


        /// <summary> Parses the raw JSON string, wrapping any <see cref="JsonException"/> with the file path as context. </summary>
        /// <param name="json"> The raw JSON text. </param>
        /// <param name="filePath"> The originating file path, used only in error context. </param>
        /// <returns> The parsed <see cref="JsonDocument"/>. </returns>
        /// <exception cref="JsonException"> Thrown with <paramref name="filePath"/> as context when parsing fails. </exception>
        private static JsonDocument ParseJson(String json, String filePath)
        {
            JsonDocument? document;

            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"World definition file '{filePath}' contains invalid JSON: {ex.Message}", ex);
            }

            return document;
        }


        /// <summary> Reads and validates the <c>rooms</c> array from the root element. </summary>
        /// <param name="root"> The root JSON element of the world definition document. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> An ordered list of validated room instance specs. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the <c>rooms</c> array is absent, empty, or any entry fails validation. </exception>
        private static IReadOnlyList<RoomInstanceSpec> ReadRooms(JsonElement root, String filePath)
        {
            Boolean hasRooms = root.TryGetProperty("rooms", out JsonElement roomsElement);

            if (!hasRooms || roomsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': the 'rooms' array is missing.");
            }

            if (roomsElement.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': the 'rooms' array is empty. A world definition must declare at least one room.");
            }

            List<RoomInstanceSpec> rooms   = new List<RoomInstanceSpec>();
            HashSet<String>        seenIds = new HashSet<String>();
            Int32                  index   = 0;

            foreach (JsonElement entry in roomsElement.EnumerateArray())
            {
                RoomInstanceSpec spec = ReadRoomEntry(entry, index, filePath, seenIds);
                rooms.Add(spec);
                seenIds.Add(spec.Id);
                index++;
            }

            return rooms;
        }


        /// <summary> Reads and validates a single room entry from the <c>rooms</c> array. </summary>
        /// <param name="entry"> The JSON object for this room entry. </param>
        /// <param name="index"> The zero-based position in the array, used in error messages when the <c>id</c> field is absent. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <param name="seenIds"> The set of room ids already validated; used to detect duplicates. </param>
        /// <returns> A validated <see cref="RoomInstanceSpec"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>id</c> or <c>prefab</c> is absent or blank, or when the id is a duplicate. </exception>
        private static RoomInstanceSpec ReadRoomEntry(
            JsonElement     entry,
            Int32           index,
            String          filePath,
            HashSet<String> seenIds)
        {
            Boolean hasId   = entry.TryGetProperty("id", out JsonElement idElement);
            String? id      = hasId ? idElement.GetString() : null;
            Boolean idValid = !String.IsNullOrWhiteSpace(id);

            if (!idValid)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': room at index {index} is missing a non-blank 'id' field.");
            }

            Boolean duplicate = seenIds.Contains(id!);

            if (duplicate)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': duplicate room id '{id}'. Each room instance must have a unique id.");
            }

            Boolean hasPrefab   = entry.TryGetProperty("prefab", out JsonElement prefabElement);
            String? prefabName  = hasPrefab ? prefabElement.GetString() : null;
            Boolean prefabValid = !String.IsNullOrWhiteSpace(prefabName);

            if (!prefabValid)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': room '{id}' is missing a non-blank 'prefab' field.");
            }

            IReadOnlyList<String> entityPrefabNames = ReadEntityPrefabNames(entry, id!, filePath);

            return new RoomInstanceSpec(id!, prefabName!, entityPrefabNames);
        }


        /// <summary> Reads the optional <c>entities</c> string array from a room entry. </summary>
        /// <param name="entry"> The JSON object for the room entry. </param>
        /// <param name="roomId"> The room id, used to enrich error messages. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <returns> The list of entity prefab names; empty when the <c>entities</c> property is absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the <c>entities</c> property is present but is not a JSON array, or when an array element is not a non-blank string. </exception>
        private static IReadOnlyList<String> ReadEntityPrefabNames(JsonElement entry, String roomId, String filePath)
        {
            Boolean hasEntities = entry.TryGetProperty("entities", out JsonElement entitiesElement);

            List<String> result;

            if (!hasEntities)
            {
                result = new List<String>();
            }
            else
            {
                if (entitiesElement.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException($"World definition file '{filePath}': room '{roomId}' has an 'entities' property that is not an array.");
                }

                result = new List<String>();
                Int32 entityIndex = 0;

                foreach (JsonElement element in entitiesElement.EnumerateArray())
                {
                    String? name      = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
                    Boolean nameValid = !String.IsNullOrWhiteSpace(name);

                    if (!nameValid)
                    {
                        throw new InvalidOperationException($"World definition file '{filePath}': room '{roomId}' entities entry at index {entityIndex} is not a non-blank string.");
                    }

                    result.Add(name!);
                    entityIndex++;
                }
            }

            return result;
        }


        /// <summary> Builds a set of all declared room instance ids for use when validating connection endpoints. </summary>
        /// <param name="rooms"> The validated list of room instance specs. </param>
        /// <returns> A set containing every room id. </returns>
        private static HashSet<String> BuildIdSet(IReadOnlyList<RoomInstanceSpec> rooms)
        {
            HashSet<String> ids = new HashSet<String>();

            foreach (RoomInstanceSpec room in rooms)
            {
                ids.Add(room.Id);
            }

            return ids;
        }


        /// <summary> Reads and validates the optional <c>connections</c> array from the root element. </summary>
        /// <param name="root"> The root JSON element of the world definition document. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <param name="knownIds"> The set of declared room ids; used to validate connection endpoints. </param>
        /// <returns> An ordered list of validated connection specs; empty when the <c>connections</c> property is absent. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when a connection entry fails validation. </exception>
        private static IReadOnlyList<ConnectionSpec> ReadConnections(
            JsonElement     root,
            String          filePath,
            HashSet<String> knownIds)
        {
            Boolean hasConnections = root.TryGetProperty("connections", out JsonElement connectionsElement);

            List<ConnectionSpec> result;

            if (!hasConnections)
            {
                result = new List<ConnectionSpec>();
            }
            else
            {
                if (connectionsElement.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException($"World definition file '{filePath}': the 'connections' property is not an array.");
                }

                result = new List<ConnectionSpec>();
                Int32 index = 0;

                foreach (JsonElement entry in connectionsElement.EnumerateArray())
                {
                    ConnectionSpec spec = ReadConnectionEntry(entry, index, filePath, knownIds);
                    result.Add(spec);
                    index++;
                }
            }

            return result;
        }


        /// <summary> Reads and validates a single connection entry from the <c>connections</c> array. </summary>
        /// <param name="entry"> The JSON object for this connection entry. </param>
        /// <param name="index"> The zero-based position in the array, used in error messages. </param>
        /// <param name="filePath"> Used to enrich error messages. </param>
        /// <param name="knownIds"> The set of declared room ids; endpoint ids must be members of this set. </param>
        /// <returns> A validated <see cref="ConnectionSpec"/>. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when <c>from</c> or <c>to</c> is absent, blank, undeclared, or when both endpoints share the same id. </exception>
        private static ConnectionSpec ReadConnectionEntry(
            JsonElement     entry,
            Int32           index,
            String          filePath,
            HashSet<String> knownIds)
        {
            Boolean hasFrom  = entry.TryGetProperty("from", out JsonElement fromElement);
            String? fromId   = hasFrom ? fromElement.GetString() : null;
            Boolean fromValid = !String.IsNullOrWhiteSpace(fromId);

            if (!fromValid)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': connection at index {index} is missing a non-blank 'from' field.");
            }

            Boolean hasTo    = entry.TryGetProperty("to", out JsonElement toElement);
            String? toId     = hasTo ? toElement.GetString() : null;
            Boolean toValid  = !String.IsNullOrWhiteSpace(toId);

            if (!toValid)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': connection at index {index} is missing a non-blank 'to' field.");
            }

            Boolean isSelfConnection = fromId!.Equals(toId, StringComparison.Ordinal);

            if (isSelfConnection)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': connection at index {index} connects room '{fromId}' to itself. Self-connections are not permitted.");
            }

            Boolean fromKnown = knownIds.Contains(fromId);

            if (!fromKnown)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': connection at index {index} references unknown room id '{fromId}' in 'from'.");
            }

            Boolean toKnown = knownIds.Contains(toId!);

            if (!toKnown)
            {
                throw new InvalidOperationException($"World definition file '{filePath}': connection at index {index} references unknown room id '{toId}' in 'to'.");
            }

            return new ConnectionSpec(fromId, toId!);
        }
    }
}
