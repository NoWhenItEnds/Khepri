using Godot;
using Khepri.Entities;
using Khepri.Entities.Components;
using System;

namespace Khepri.UI.World.Tabs.Character
{
    public partial class InventoryElement : VBoxContainer
    {
        /// <summary> Force the display panel to reflect the current game's state. </summary>
        /// <param name="selectedEntity"> The entity whose character sheet is shown; when null the sheet is left blank. </param>
        public void ForceUpdate(Entity selectedEntity)
        {
            InventoryComponent? inventory = selectedEntity.GetComponent<InventoryComponent>();
            Boolean isVisible = inventory != null && inventory.GetEntities().Count > 0;
            Visible = isVisible;

            if (isVisible)
            {
                foreach (Entity item in inventory!.GetEntities())
                {
                    // TODO - Add.
                    //Label row = new Label();
                    //row.Text = item.GetName().ToCapitalised();
                    //_column.AddChild(row);
                }
            }
        }
    }
}
