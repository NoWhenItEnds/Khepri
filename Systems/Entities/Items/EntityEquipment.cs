using Godot;
using Khepri.Resources;
using Khepri.Resources.Items;
using Khepri.Types.Exceptions;
using System;
using System.Collections.Generic;

namespace Khepri.Entities.Items
{
    /// <summary> A data structure representing an entity's equipment slots. </summary>
    public class EntityEquipment
    {
        /// <summary> The outfit the entity is currently wearing. </summary>
        public OutfitResource? Outfit { get; private set; } = null;

        /// <summary> The accessories the entity is currently wearing. </summary>
        /// <remarks> The size of the array indicates how many they can wear at a time. </remarks>
        public AccessoryResource?[] Accessories { get; private set; } = new AccessoryResource?[3];

        /// <summary> The tools the entity currently have equipped. They can switch between them without needing to reequip them. </summary>
        public ToolResource?[] Tools { get; private set; } = new ToolResource?[2];


        /// <summary> The index of the tool the entity is currently wielding. </summary>
        /// <remarks> If the index is at a spot there is no tool, then the entity has open hands. </remarks>
        private Int32 _currentToolIndex = 0;


        /// <summary> Sets the entity's currently equipped outfit, returning the previous outfit if there was indeed one. </summary>
        /// <param name="outfit"> The outfit to equip. </param>
        /// <returns> A reference to the formally equipped outfit, or a null if there wasn't one. </returns>
        public OutfitResource? SetOutfit(OutfitResource outfit)
        {
            OutfitResource? previousOutfit = RemoveOutfit();
            Outfit = outfit;
            return previousOutfit;
        }


        /// <summary> Remove and return the currently equipped outfit. A null indicates that there wasn't one equipped. </summary>
        /// <returns> A reference to the formally equipped outfit. </returns>
        public OutfitResource? RemoveOutfit()
        {
            OutfitResource? outfit = Outfit;
            Outfit = null;
            return outfit;
        }


        /// <summary> Sets one of the entity's currently equipped accessories, returning the previous accessory if there was one at the index already. </summary>
        /// <param name="accessory"> The accessory to equip. </param>
        /// <returns> A reference to the formally equipped accessory, or a null if there wasn't one. </returns>
        public AccessoryResource? SetAccessory(AccessoryResource accessory)
        {
            Int32 index = 0;
            for (Int32 i = 0; i < Accessories.Length; i++)
            {
                if (Accessories[i] == null)
                {
                    index = i;
                    break;
                }
            }

            return SetAccessory(accessory, index);
        }


        /// <summary> Sets one of the entity's currently equipped accessories, returning the previous accessory if there was one at the index already. </summary>
        /// <param name="accessory"> The accessory to equip. </param>
        /// <param name="index"> The position to add it at. </param>
        /// <returns> A reference to the formally equipped accessory, or a null if there wasn't one. </returns>
        public AccessoryResource? SetAccessory(AccessoryResource accessory, Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Accessories.Length);

            AccessoryResource? previousAccessory = RemoveAccessory(index);
            Accessories[index] = accessory;
            return previousAccessory;
        }


        /// <summary> Remove and return a currently equipped accessory. A null indicates that there wasn't one equipped. </summary>
        /// <param name="index"> The position to remove. </param>
        /// <returns> A reference to the formally equipped accessory. </returns>
        public AccessoryResource? RemoveAccessory(Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Accessories.Length);

            AccessoryResource? accessory = Accessories[index];
            Accessories[index] = null;
            return accessory;
        }


        /// <summary> Sets one of the entity's currently equipped tools, returning the previous tool if there was one at the index already. </summary>
        /// <param name="tool"> The tool to equip. </param>
        /// <returns> A reference to the formally equipped tool, or a null if there wasn't one. </returns>
        public ToolResource? SetTool(ToolResource tool)
        {
            Int32 index = 0;
            for (Int32 i = 0; i < Tools.Length; i++)
            {
                if (Tools[i] == null)
                {
                    index = i;
                    break;
                }
            }

            return SetTool(tool, index);
        }


        /// <summary> Sets one of the entity's currently equipped tools, returning the previous tool if there was one at the index already. </summary>
        /// <param name="tool"> The tool to equip. </param>
        /// <param name="index"> The position to add it at. </param>
        /// <returns> A reference to the formally equipped tool, or a null if there wasn't one. </returns>
        public ToolResource? SetTool(ToolResource tool, Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Tools.Length);

            ToolResource? previousTool = RemoveTool(index);
            Tools[index] = tool;
            return previousTool;
        }


        /// <summary> Remove and return a currently equipped tool. A null indicates that there wasn't one equipped. </summary>
        /// <param name="index"> The position to remove. </param>
        /// <returns> A reference to the formally equipped tool. </returns>
        public ToolResource? RemoveTool(Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Tools.Length);

            ToolResource? tools = Tools[index];
            Tools[index] = null;
            return tools;
        }


        /// <summary> Toggle which of the two tools the entity has equipped. </summary>
        /// <returns> Returns a reference to the current tool. A null indicates open hands. </returns>
        public ToolResource? ToggleEquippedTool()
        {
            _currentToolIndex = _currentToolIndex == 0 ? 1 : 0;
            return GetCurrentTool();
        }


        /// <summary> Get the tool that is currently equipped. </summary>
        /// <returns> Returns a reference to the current tool. A null indicates open hands. </returns>
        public ToolResource? GetCurrentTool() => Tools[_currentToolIndex];


        /// <summary> Package the equipment's state for saving. </summary>
        /// <returns> The state packaged as key value pairs. </returns>
        public Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>> Serialise()
        {
            Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>> data = new Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>>();
            if (Outfit != null)
            {
                data.Add("outfit", Outfit.Serialise());
            }

            for (Int32 i = 0; i < Accessories.Length; i++)
            {
                AccessoryResource? accessory = Accessories[i];
                if (accessory != null)
                {
                    data.Add($"accessory_{i}", accessory.Serialise());
                }
            }

            for (Int32 i = 0; i < Tools.Length; i++)
            {
                ToolResource? tool = Tools[i];
                if (tool != null)
                {
                    data.Add($"tool_{i}", tool.Serialise());
                }
            }

            return data;
        }


        /// <summary> Unpackage the stored data to reconstruct the equipment. </summary>
        /// <param name="data"> The packed data needing unpacking. </param>
        public void Deserialise(Godot.Collections.Dictionary<String, Godot.Collections.Dictionary<String, Variant>> data)
        {
            ResourceController resourceController = ResourceController.Instance;
            foreach (KeyValuePair<String, Godot.Collections.Dictionary<String, Variant>> item in data)
            {
                if (item.Value.TryGetValue("id", out Variant id))
                {
                    String[] key = item.Key.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if(Int32.TryParse(key[1], out Int32 index))
                    {
                        switch(key[0])
                        {
                            case "outfit":
                                OutfitResource outfit = resourceController.CreateResource<OutfitResource>((String)id);
                                outfit.Deserialise(item.Value);
                                SetOutfit(outfit);
                                break;
                            case "accessory":
                                AccessoryResource accessory = resourceController.CreateResource<AccessoryResource>((String)id);
                                accessory.Deserialise(item.Value);
                                SetAccessory(accessory, index);
                                break;
                            case "tool":
                                ToolResource tool = resourceController.CreateResource<ToolResource>((String)id);
                                tool.Deserialise(item.Value);
                                SetTool(tool, index);
                                break;
                            default:
                                throw new ItemException($"The equipment could not deserialise an item with the key prefix '{key[0]}'.");
                        }
                    }
                }
                else
                {
                    throw new ItemException("Every item needs an 'id' field. I just tried to deserialise an equipment item without one.");
                }
            }
        }
    }
}
