using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using Jaypen.Utilities.Extensions;
using Khepri.Entities;
using Khepri.Entities.Components;
using Khepri.Entities.Components.Skills;

namespace Khepri.UI.World.Tabs.Character
{
    /// <summary> A panel used to show the currently selected entity's character sheet. </summary>
    public partial class CharacterPanel : ScrollContainer
    {
        /// <summary> The node that holds all the sub-components in the character panel. </summary>
        [ExportGroup("Nodes")]
        [Export] private VBoxContainer _column = null!;

        [Export] private RichTextLabel _headingLabel = null!;

        [Export] private ConditionElement _conditionElement = null!;

        [Export] private SkillElement _skillElement = null!;

        [Export] private InventoryElement _inventoryElement = null!;


        /// <summary> A digest of the state last rendered, so an unchanged per-frame update skips the rebuild that would otherwise reset the scroll position. </summary>
        private String _signature = String.Empty;


        /// <summary> Force the display panel to reflect the current game's state. </summary>
        /// <param name="selectedEntity"> The entity whose character sheet is shown; when null the sheet is left blank. </param>
        public void ForceUpdate(Entity selectedEntity)
        {
            if (Visible)
            {
                SetHeading(selectedEntity);
                _conditionElement.ForceUpdate(selectedEntity);
                _skillElement.ForceUpdate(selectedEntity);
                _inventoryElement.ForceUpdate(selectedEntity);
            }
        }


        /// <summary> Set's the character sheet's heading. </summary>
        /// <param name="entity"> The entity being described. </param>
        private void SetHeading(Entity entity)
        {
            _headingLabel.Text = entity.GetName().ToCapitalised();
        }
    }
}
