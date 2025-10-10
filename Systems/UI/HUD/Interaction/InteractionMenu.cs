using Godot;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Entities.Items;
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


        /// <summary> A reference to the player controller singleton. </summary>
        private PlayerController _playerController;


        private const String LABEL_FORMAT = "[color={1}]{0}[/color]";


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            StringBuilder builder = new StringBuilder();
            IEntity? selectedEntity = _playerController.GetCurrentInteractable();
            if (selectedEntity != null)
            {

                foreach (IEntity entity in _playerController.PlayerUnit.UsableEntities)
                {
                    String text = "Placeholder";
                    switch (entity)
                    {
                        case ItemNode item:
                            text = item.Resource.Id;
                            break;
                    }

                    builder.AppendLine(String.Format(LABEL_FORMAT, text.Capitalize(), entity == selectedEntity ? Colors.Green.ToHtml() : Colors.Red.ToHtml()));
                }
            }
            _label.Text = builder.ToString();
        }
    }
}
