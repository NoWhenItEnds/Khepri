using Godot;
using Khepri.Entities.Actors;
using Khepri.Entities.Actors.Components;
using Khepri.Models.GOAP;
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


        private AgentController _controller;

        /// <summary> The unit this menu represents. </summary>
        private Unit _unit;

        /// <summary> An internal reference to the game's viewport. </summary>
        private Viewport _viewport;


        /// <summary> The format to use to show unit needs. </summary>
        private const String NEEDS_FORMAT = "HEA: {0:F1}\nHUG: {1:F1}\nFAT: {2:F1}\nENT: {3:F1}\nSTA: {4:F1}";

        /// <summary> The format to use to show unit beliefs. </summary>
        private const String BELIEF_FORMAT = "{0}: {1}";


        /// <inheritdoc/>
        public override void _Ready()
        {
            _viewport = GetViewport();
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
            foreach (KeyValuePair<String, AgentBelief> beliefs in _controller.AvailableBeliefs)
            {
                beliefBuilder.AppendLine(String.Format(BELIEF_FORMAT, beliefs.Key, beliefs.Value.Evaluate()));
            }
            _beliefsLabel.Text = beliefBuilder.ToString();
        }
    }
}
