using Godot;
using Khepri.Controllers;
using System;

namespace Khepri.Entities.Actors.Components
{
    /// <summary> The lights used by a unit to represent its line of sight. </summary>
    public partial class UnitLights : Node3D
    {
        /// <summary> A reference to the unit. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _unit;

        /// <summary> The light used to represent a direct line of sight. </summary>
        [Export] private SpotLight3D _spotlight;

        /// <summary> The light used to represent awareness around the unit. </summary>
        [Export] private OmniLight3D _omniLight;


        /// <inheritdoc/>
        public override void _Ready()
        {
            ToggleLights(PlayerController.Instance.PlayerUnit == _unit);
        }


        /// <summary> Toggle whether the lights are active. </summary>
        /// <param name="isActive"> Whether the visibility lights should be active or not. </param>
        public void ToggleLights(Boolean isActive)
        {
            _spotlight.Visible = isActive;
            _omniLight.Visible = isActive;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _spotlight.GlobalRotationDegrees = new Vector3(0f, _unit.Direction * -1f - 90f, 0f);
        }
    }
}
