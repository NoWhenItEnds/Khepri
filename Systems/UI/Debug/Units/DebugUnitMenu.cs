using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.Entities.Actors.Components;
using Khepri.Entities.Devices;
using Khepri.Entities.Items;
using Khepri.GOAP;
using System;
using System.Collections.Generic;
using System.Text;

namespace Khepri.UI.Debug.Units
{
    /// <summary> A menu showing debug information about a specific unit. </summary>
    public partial class DebugUnitMenu : Control
    {
        /// <summary> A reference to the label showing the unit's needs. </summary>
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _needsLabel;

        /// <summary> A reference to the label showing the unit's GOAP beliefs. </summary>
        [Export] private RichTextLabel _beliefsLabel;

        /// <summary> A reference to the label showing the unit's sensor knowledge. </summary>
        [Export] private RichTextLabel _sensorsLabel;


        private AgentController _controller;

        /// <summary> The unit this menu represents. </summary>
        private Unit _unit;

        /// <summary> An internal reference to the game's viewport. </summary>
        private Viewport _viewport;

        /// <summary> A reference to the world controller. </summary>
        private WorldController _worldController;


        /// <summary> The format to use to show unit needs. </summary>
        private const String NEEDS_FORMAT = "HEA: {0:F1}\nHUG: {1:F1}\nFAT: {2:F1}\nENT: {3:F1}\nSTA: {4:F1}";

        /// <summary> The format to use to show unit beliefs. </summary>
        private const String BELIEF_FORMAT = "[color={1}]- {0}[/color]";

        /// <summary> The format to use to show sensor information. </summary>
        private const String SENSOR_FORMAT = "[b]{0}[/b] <{1:F1}, {2:F1}, {3:F1}> {4:F1}m";


        /// <inheritdoc/>
        public override void _Ready()
        {
            _viewport = GetViewport();
            _worldController = WorldController.Instance;
        }


        /// <summary> Construct the menu. </summary>
        /// <param name="unit"> The unit this menu represents. </param>
        public void Initialise(AgentController controller, Unit unit)
        {
            _controller = controller;
            _unit = unit;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Follow the unit around.
            Vector2 screenPosition = _viewport.GetCamera3D().UnprojectPosition(_unit.GlobalPosition);
            GlobalPosition = screenPosition;

            // Update information.
            NeedComponent needs = _unit.Needs;
            _needsLabel.Text = String.Format(NEEDS_FORMAT, needs.CurrentHealth, needs.CurrentHunger, needs.CurrentFatigue, needs.CurrentEntertainment, needs.CurrentStamina);

            // Update Beliefs.
            StringBuilder beliefBuilder = new StringBuilder();
            beliefBuilder.Append("[font_size=10]");
            foreach (KeyValuePair<String, AgentBelief> beliefs in _controller.AvailableBeliefs)
            {
                beliefBuilder.AppendLine(String.Format(BELIEF_FORMAT, beliefs.Key, beliefs.Value.Evaluate() ? "green" : "red"));
            }
            _beliefsLabel.Text = beliefBuilder.ToString();

            // Update sensors.
            DateTimeOffset currentTime = _worldController.CurrentTime;
            StringBuilder sensorBuilder = new StringBuilder();
            sensorBuilder.Append("[font_size=10]");
            foreach (KnownEntity entity in _unit.Sensors.GetEntities())
            {
                Vector3 pos = entity.LastKnownPosition;
                Double minutes = (currentTime - entity.LastSeenTimestamp).TotalMinutes;
                switch (entity.Entity)
                {
                    case Unit unit:
                        sensorBuilder.AppendLine(String.Format(SENSOR_FORMAT, unit.Name, pos.X, pos.Y, pos.Z, minutes));
                        break;
                    case ItemNode item:
                        sensorBuilder.AppendLine(String.Format(SENSOR_FORMAT, item.Resource.Id, pos.X, pos.Y, pos.Z, minutes));
                        break;
                    case DeviceNode device:
                        sensorBuilder.AppendLine(String.Format(SENSOR_FORMAT, device.Name, pos.X, pos.Y, pos.Z, minutes));
                        break;
                }

            }
            _sensorsLabel.Text = sensorBuilder.ToString();
        }
    }
}
