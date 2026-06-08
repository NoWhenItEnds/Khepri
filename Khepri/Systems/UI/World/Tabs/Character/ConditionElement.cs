using System;
using Godot;
using Khepri.Entities;
using Khepri.Entities.Components;

namespace Khepri.UI.World.Tabs.Character
{
    public partial class ConditionElement : VBoxContainer
    {
        [ExportGroup("Nodes")]
        [Export] private ProgressBar _healthProgressBar = null!;


        /// <summary> Force the display panel to reflect the current game's state. </summary>
        /// <param name="selectedEntity"> The entity whose character sheet is shown; when null the sheet is left blank. </param>
        public void ForceUpdate(Entity selectedEntity)
        {
            ConditionComponent? condition = selectedEntity.GetComponent<ConditionComponent>();
            Boolean isVisible = condition != null;
            Visible = isVisible;

            if (isVisible)
            {
                _healthProgressBar.MaxValue = condition!.Max;   // Can't be null here.
                _healthProgressBar.Value = condition.Current;
            }
        }
    }
}
