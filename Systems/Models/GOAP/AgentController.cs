using Godot;
using Khepri.Entities;
using System;
using System.Collections.Generic;

namespace Khepri.Models.GOAP
{
    /// <summary> Uses a GOAP implementation to control an entity. The AI brain that controls a unit. </summary>
    public partial class AgentController : Node
    {
        /// <summary> The current goal the agent is trying to accomplish. </summary>
        public AgentGoal CurrentGoal { get; private set; }

        /// <summary> The current plan the agent is using to address its current goal. </summary>
        public ActionPlan CurrentPlan { get; private set; }

        /// <summary> The current action the agent is in the process of doing. </summary>
        public AgentAction CurrentAction { get; private set; }


        /// <summary> An ordered array of the previous goals the agent tried to accomplish. </summary>
        /// <remarks> [0] is the latest. [^1] is the oldest. </remarks>
        private AgentGoal[] _previousGoals = new AgentGoal[10];

        /// <summary> The 'truths' the agent knows. The beliefs it has about the world state. </summary>
        private Dictionary<String, AgentBelief> _beliefs = new Dictionary<string, AgentBelief>();

        /// <summary> The goals that the agent will seek to address. </summary>
        private HashSet<AgentGoal> _availableGoals;

        /// <summary> The potential actions this agent has access to. </summary>
        private HashSet<AgentAction> _availableAction;

        /// <summary> The entity this controller is responsible for controlling. </summary>
        private Unit _controlledEntity;


        /// <inheritdoc/>
        public override void _Ready()
        {
            base._Ready();
        }


        /// <summary> Set's the agent's initial beliefs. </summary>
        private void InitialiseBeliefs()    // TODO - These should be pulled from a JSON or something.
        {
            BeliefFactory factory = new BeliefFactory(_controlledEntity, _beliefs);

            factory.AddBelief("nothing", () => false);
        }



        /// <summary> Adds the current goal to the array of previous goals. </summary>
        public void ArchiveCurrentGoal()
        {
            AgentGoal[] newValues = new AgentGoal[10];
            newValues[0] = CurrentGoal;
            Array.Copy(_previousGoals, 0, newValues, 1, _previousGoals.Length - 1);
            _previousGoals = newValues;
        }
    }
}
