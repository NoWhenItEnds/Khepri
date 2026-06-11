using System;
using Godot;
using Khepri.Entities;
using Khepri.Entities.Components;

namespace Khepri.UI.World.Tabs.Character
{
    public partial class ConditionElement : VBoxContainer
    {
        /// <summary> The progress bar that displays the character's health. </summary>
        [ExportGroup("Nodes")]
        [Export] private ProgressBar _healthProgressBar = null!;


        /// <summary> Force the display panel to reflect the current game's state. </summary>
        /// <param name="selectedEntity"> The entity whose character sheet is shown; when null the sheet is left blank. </param>
        public void ForceUpdate(Entity selectedEntity)
        {
            if (selectedEntity.TryGetComponent(out ConditionComponent? condition))
            {
                Visible = true;
                _healthProgressBar.MaxValue = condition.StaminaMaximum;
                _healthProgressBar.Value = condition.StaminaCurrent;
            }
            else
            {
                Visible = false;
            }
        }
    }
}
