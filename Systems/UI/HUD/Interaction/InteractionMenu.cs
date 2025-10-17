using Godot;
using Khepri.Entities;
using Khepri.Entities.Actors;
using Khepri.Entities.Devices;
using Khepri.Entities.Items;
using Khepri.Resources.Devices;
using Khepri.Resources.Items;
using System;
using System.Text;

namespace Khepri.UI.HUD.Interaction
{
    /// <summary> A menu that displays interactable elements. </summary>
    public partial class InteractionMenu : Control
    {
        /// <summary> The label used to represent the nearby objects. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _label;


        /// <summary> The format to use for the interaction item labels. </summary>
        private const String LABEL_FORMAT = "[color={1}]{0}[/color]";


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            StringBuilder builder = new StringBuilder();
            IEntity? selectedEntity = ActorController.Instance.GetPlayerInteractable();
            if (selectedEntity != null)
            {

                foreach (IEntity entity in ActorController.Instance.GetPlayer().UsableEntities)
                {
                    String text = "Placeholder";
                    switch (entity)
                    {
                        case ItemNode item:
                            text = item.GetResource<ItemResource>().Id;
                            break;
                        case DeviceNode device:
                            text = device.GetResource<DeviceResource>().Id;
                            break;
                    }

                    builder.AppendLine(String.Format(LABEL_FORMAT, text.Capitalize(), entity == selectedEntity ? Colors.Green.ToHtml() : Colors.Red.ToHtml()));
                }
            }
            _label.Text = builder.ToString();
        }
    }
}
