using Khepri.Data.Items;
using System;

namespace Khepri.Entities.Items
{
    /// <summary> A data structure representing an entity's equipment slots. </summary>
    public class EntityEquipment
    {
        /// <summary> The outfit the entity is currently wearing. </summary>
        public OutfitData? Outfit { get; private set; } = null;

        /// <summary> The accessories the entity is currently wearing. </summary>
        /// <remarks> The size of the array indicates how many they can wear at a time. </remarks>
        public AccessoryData?[] Accessories { get; private set; } = new AccessoryData?[3];

        /// <summary> The tools the entity currently have equipped. They can switch between them without needing to reequip them. </summary>
        public ToolData?[] Tools { get; private set; } = new ToolData?[2];


        /// <summary> The index of the tool the entity is currently wielding. </summary>
        /// <remarks> If the index is at a spot there is no tool, then the entity has open hands. </remarks>
        private Int32 _currentToolIndex = 0;


        /// <summary> Sets the entity's currently equipped outfit, returning the previous outfit if there was indeed one. </summary>
        /// <param name="outfit"> The outfit to equip. </param>
        /// <returns> A reference to the formally equipped outfit, or a null if there wasn't one. </returns>
        public OutfitData? SetOutfit(OutfitData outfit)
        {
            OutfitData? previousOutfit = RemoveOutfit();
            Outfit = outfit;
            return previousOutfit;
        }


        /// <summary> Remove and return the currently equipped outfit. A null indicates that there wasn't one equipped. </summary>
        /// <returns> A reference to the formally equipped outfit. </returns>
        public OutfitData? RemoveOutfit()
        {
            OutfitData? outfit = Outfit;
            Outfit = null;
            return outfit;
        }


        /// <summary> Sets one of the entity's currently equipped accessories, returning the previous accessory if there was one at the index already. </summary>
        /// <param name="accessory"> The accessory to equip. </param>
        /// <returns> A reference to the formally equipped accessory, or a null if there wasn't one. </returns>
        public AccessoryData? SetAccessory(AccessoryData accessory)
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
        public AccessoryData? SetAccessory(AccessoryData accessory, Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Accessories.Length);

            AccessoryData? previousAccessory = RemoveAccessory(index);
            Accessories[index] = accessory;
            return previousAccessory;
        }


        /// <summary> Remove and return a currently equipped accessory. A null indicates that there wasn't one equipped. </summary>
        /// <param name="index"> The position to remove. </param>
        /// <returns> A reference to the formally equipped accessory. </returns>
        public AccessoryData? RemoveAccessory(Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Accessories.Length);

            AccessoryData? accessory = Accessories[index];
            Accessories[index] = null;
            return accessory;
        }


        /// <summary> Sets one of the entity's currently equipped tools, returning the previous tool if there was one at the index already. </summary>
        /// <param name="tool"> The tool to equip. </param>
        /// <returns> A reference to the formally equipped tool, or a null if there wasn't one. </returns>
        public ToolData? SetTool(ToolData tool)
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
        public ToolData? SetTool(ToolData tool, Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Tools.Length);

            ToolData? previousTool = RemoveTool(index);
            Tools[index] = tool;
            return previousTool;
        }


        /// <summary> Remove and return a currently equipped tool. A null indicates that there wasn't one equipped. </summary>
        /// <param name="index"> The position to remove. </param>
        /// <returns> A reference to the formally equipped tool. </returns>
        public ToolData? RemoveTool(Int32 index)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Tools.Length);

            ToolData? tools = Tools[index];
            Tools[index] = null;
            return tools;
        }


        /// <summary> Toggle which of the two tools the entity has equipped. </summary>
        /// <returns> Returns a reference to the current tool. A null indicates open hands. </returns>
        public ToolData? ToggleEquippedTool()
        {
            _currentToolIndex = _currentToolIndex == 0 ? 1 : 0;
            return GetCurrentTool();
        }


        /// <summary> Get the tool that is currently equipped. </summary>
        /// <returns> Returns a reference to the current tool. A null indicates open hands. </returns>
        public ToolData? GetCurrentTool() => Tools[_currentToolIndex];
    }
}
