using Godot;
using Khepri.Entities.Actors;
using Khepri.GOAP;
using System;
using System.Collections.Generic;

namespace Khepri.UI.Debug.Units
{
    /// <summary> The window used to display debug information about all units on screen. </summary>
    public partial class DebugUnitWindow : Control
    {
        /// <summary> The prefab to use when spawning unit menus. </summary>
        [ExportGroup("Prefabs")]
        [Export] private PackedScene _unitMenuPrefab;

        [Export] private AgentController _enemyController;   // TODO - REPLACE!!!

        [Export] private Being _enemyUnit;   // TODO - REPLACE!!!


        /// <summary> Whether the window should be rendered. </summary>
        [ExportGroup("Settings")]
        [Export] private Boolean _showDebug = false;

        /// <summary> Whether a debug element should be rendered for the player. </summary>
        [Export] private Boolean _showPlayer = false;


        /// <summary> A reference to all the currently visible units and their active menus on the screen. </summary>
        private Dictionary<Being, DebugUnitMenu> _activeMenus = new Dictionary<Being, DebugUnitMenu>();


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // TODO - Pull all the units from the central unit factory when it's created.
            if (_showDebug && _enemyUnit.VisibilityNotifier.IsOnScreen())
            {
                if (!_activeMenus.ContainsKey(_enemyUnit))
                {
                    CreateMenu(_enemyUnit);
                }
            }
            else
            {
                if (_activeMenus.ContainsKey(_enemyUnit))
                {
                    FreeMenu(_enemyUnit);
                }
            }
        }

        private void CreateMenu(Being unit)
        {
            DebugUnitMenu? window = _unitMenuPrefab.InstantiateOrNull<DebugUnitMenu>();
            if (window != null)
            {
                AddChild(window);
                window.Initialise(_enemyController, unit);
                _activeMenus.Add(unit, window);
            }
        }

        private void FreeMenu(Being unit)
        {
            DebugUnitMenu? window = _activeMenus[unit];
            if (window != null)
            {
                window.QueueFree();
            }
            _activeMenus.Remove(unit);
        }
    }
}
