using Godot;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Entities.Devices;
using Khepri.Entities.Items;
using System;

namespace Khepri.UI.HUD.Interaction
{
    /// <summary> An item in the interaction UI representing an interactable element in the game world. </summary>
    public partial class InteractionItem : Control
    {
        /// <summary> A reference to the sprite used to render the interaction button. </summary>
        [ExportGroup("Nodes")]
        [Export] private AnimatedSprite2D _sprite;

        /// <summary> A reference to the label to display the interaction text. </summary>
        [Export] private RichTextLabel _label;


        /// <summary> A reference to the entity this item represents. </summary>
        public IEntity Entity { get; private set; }

        /// <summary> A reference to the player controller singleton. </summary>
        private PlayerController _playerController;


        /// <summary> The format to use when rendering an item label. </summary>
        private const String ITEM_LABEL_FORMAT = "Pickup {0}";

        /// <summary> The format to use when rendering an device label. </summary>
        private const String DEVICE_LABEL_FORMAT = "Use {0}";


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
            _sprite.Play();
        }


        /// <summary> Construct the item. </summary>
        /// <param name="entity"> A reference to the entity this item represents. </param>
        public void Initialise(IEntity entity)
        {
            Entity = entity;

            if (Entity is Item item)
            {
                _label.Text = String.Format(ITEM_LABEL_FORMAT, item.Data.Name.Capitalize());
            }
            else if (Entity is Device device)
            {
                _label.Text = String.Format(DEVICE_LABEL_FORMAT, device.Data.Name.Capitalize());
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            if (_playerController.IsUsingJoypad)
            {

            }
            else
            {
                _sprite.Animation = "E";
            }
        }
    }
}
