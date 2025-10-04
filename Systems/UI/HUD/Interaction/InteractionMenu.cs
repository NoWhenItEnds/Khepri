using Godot;
using Khepri.Controllers;
using Khepri.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.UI.HUD.Interaction
{
    /// <summary> A menu that displays interactable elements. </summary>
    public partial class InteractionMenu : Control
    {
        /// <summary> The prefab to use when spawning interaction items. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _interactionItemPrefab;

        /// <summary> A reference to the container the interaction items will be spawned inside. </summary>
        [ExportGroup("Nodes")]
        [Export] private VBoxContainer _container;


        /// <summary> A reference to the player controller singleton. </summary>
        private PlayerController _playerController;

        private List<InteractionItem> _spawnedItems = new List<InteractionItem>();

        /// <summary> The current selection on the interaction menu. </summary>
        private Int32 _currentSelection = 0;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Only check if the menu is currently active.
            if (Visible)
            {
                IEntity[] usableEntities = _playerController.PlayerUnit.UsableEntities.ToArray();

                // Clear any items that represent objects no longer in range.
                InteractionItem[] danglingItems = _spawnedItems.Where(x => !usableEntities.Contains(x.Entity)).ToArray();
                foreach (InteractionItem item in danglingItems)
                {
                    item.QueueFree();
                    _spawnedItems.Remove(item);
                }

                // If there is a disconnect between the number of usable items and spawned items, we need to spawn one.
                if (usableEntities.Length > _spawnedItems.Count)
                {
                    IEnumerable<IEntity> missingEntities = usableEntities.Except(_spawnedItems.Select(x => x.Entity));
                    foreach (IEntity entity in missingEntities)
                    {
                        InteractionItem item = BuildItem(entity);
                        _spawnedItems.Add(item);
                    }
                }
            }
        }

        private InteractionItem BuildItem(IEntity entity)
        {
            InteractionItem item = _interactionItemPrefab.Instantiate<InteractionItem>();
            _container.AddChild(item);
            item.Initialise(entity);
            return item;
        }
    }
}
