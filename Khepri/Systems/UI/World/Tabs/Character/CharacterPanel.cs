using System;
using Godot;
using Jaypen.Utilities.Extensions;
using Khepri.Entities;

namespace Khepri.UI.World.Tabs.Character
{
    /// <summary> A panel used to show the currently selected entity's character sheet. </summary>
    public partial class CharacterPanel : ScrollContainer
    {
        /// <summary> The node that displays the character's name. </summary>
        [ExportGroup("Nodes")]

        [Export] private RichTextLabel _headingLabel = null!;

        /// <summary> The node that represents the character's conditions. </summary>
        [Export] private ConditionElement _conditionElement = null!;

        /// <summary> The node that represents the character's skills. </summary>
        [Export] private SkillElement _skillElement = null!;

        /// <summary> The node that represents the character's inventory. </summary>
        [Export] private InventoryElement _inventoryElement = null!;


        /// <summary> A digest of the state last rendered, so an unchanged per-frame update skips the rebuild that would otherwise reset the scroll position. </summary>
        private String _signature = String.Empty;


        /// <summary> Force the display panel to reflect the current game's state. </summary>
        /// <param name="selectedEntity"> The entity whose character sheet is shown; when null the sheet is left blank. </param>
        public void ForceUpdate(Entity selectedEntity)
        {
            if (Visible)
            {
                _headingLabel.Text = selectedEntity.GetName().ToCapitalised();
                _conditionElement.ForceUpdate(selectedEntity);
                _skillElement.ForceUpdate(selectedEntity);
                _inventoryElement.ForceUpdate(selectedEntity);
            }
        }
    }
}
