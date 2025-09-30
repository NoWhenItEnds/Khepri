using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Khepri.Entities.Items
{
    /// <summary> A factory for creating items from external data. </summary>
    public static class ItemFactory
    {
        /// <summary> The relative director to the game's item data. </summary>
        private const String DATA_DIR = "res://Data/Items/";


        /// <summary> A builder for creating item data. </summary>
        /// <param name="name"> The unique identifying name or key of the item. </param>
        /// <param name="type"> What kind of item it is. </param>
        /// <returns> A reference to the create data object. </returns>
        public static ItemData Create(String name, ItemType type)
        {
            JsonElement itemElement = GetItemRoot(name, type);

            return new ItemData
            {
                UId = Guid.NewGuid(),
                Name = name,
                ItemType = type,
                SpriteIndex = GetSpriteIndex(itemElement),
                Points = GetPoints(itemElement)
            };
        }


        /// <summary> Search the data object for the item root. </summary>
        /// <param name="name"> The key / name of the object. </param>
        /// <param name="type"> The kind of object. Determines which json file to search. </param>
        /// <returns> The json element for the given item key. </returns>
        /// <exception cref="JsonException"> If the given key couldn't be found in the json file. </exception>
        private static JsonElement GetItemRoot(String name, ItemType type)
        {
            // Load json.
            String path = Path.Combine(ProjectSettings.GlobalizePath(DATA_DIR), type.ToString().ToLower() + ".json");
            String json = File.ReadAllText(path);

            JsonElement root = JsonDocument.Parse(json).RootElement;
            if (root.TryGetProperty(name, out JsonElement item))
            {
                return item;
            }
            else
            {
                throw new JsonException($"The given key '{name}' does not exist in '{path}'.");
            }
        }


        /// <summary> Get the sprite index property from the json object. </summary>
        /// <param name="itemElement"> The json element to search. </param>
        /// <returns> The index of the sprites used to represent this item. </returns>
        /// <exception cref="JsonException"> If the item doesn't contain a sprite_index property. </exception>
        private static Int32 GetSpriteIndex(JsonElement itemElement)
        {
            if (itemElement.TryGetProperty("sprite_index", out JsonElement element) && element.TryGetInt32(out Int32 spriteIndex))
            {
                return spriteIndex;
            }
            else
            {
                throw new JsonException($"The item doesn't contain a key for 'sprite_index'.");
            }
        }


        /// <summary> Get the points property from the json object. </summary>
        /// <param name="itemElement"> The json element to search. </param>
        /// <returns> Relative points representing the grid cells the item occupies in an inventory. </returns>
        /// <exception cref="JsonException"> If the item doesn't contain a points property. </exception>
        private static Vector2I[] GetPoints(JsonElement itemElement)
        {
            if (itemElement.TryGetProperty("points", out JsonElement pointsElement))
            {
                List<Vector2I> pointArray = new List<Vector2I>();
                foreach (JsonElement point in pointsElement.EnumerateArray())
                {
                    List<Int32> posArray = new List<Int32>();
                    foreach (JsonElement currentPos in point.EnumerateArray())
                    {
                        if (currentPos.TryGetInt32(out Int32 pos))
                        {
                            posArray.Add(pos);
                        }
                    }
                    pointArray.Add(new Vector2I(posArray[0], posArray[1]));
                }
                return pointArray.ToArray();
            }
            else
            {
                throw new JsonException($"The item doesn't contain a key for 'points'.");
            }
        }
    }
}
