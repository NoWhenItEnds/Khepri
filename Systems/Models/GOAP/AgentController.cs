using Godot;
using Khepri.Controllers;
using Khepri.Entities;
using Khepri.Entities.Items;
using Khepri.Models.GOAP.ActionStrategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Models.GOAP
{
    /// <summary> Uses a GOAP implementation to control an entity. The AI brain that controls a unit. </summary>
    public partial class AgentController : Node
    {
        /// <summary> The entity this controller is responsible for controlling. </summary>
        [ExportGroup("Nodes")]
        [Export] private Unit _controlledEntity;


        /// <summary> The current goal the agent is trying to accomplish. </summary>
        private AgentGoal? _currentGoal = null;

        /// <summary> The current plan the agent is using to address its current goal. </summary>
        private ActionPlan? _currentPlan = null;

        /// <summary> The current action the agent is in the process of doing. </summary>
        private AgentAction? _currentAction = null;

        /// <summary> An ordered array of the previous goals the agent tried to accomplish. </summary>
        /// <remarks> [0] is the latest. [^1] is the oldest. </remarks>
        private AgentGoal[] _previousGoals = new AgentGoal[10];

        /// <summary> The 'truths' the agent knows. The beliefs it has about the world state. </summary>
        private Dictionary<String, AgentBelief> _availableBeliefs;

        /// <summary> The goals that the agent will seek to address. </summary>
        private HashSet<AgentGoal> _availableGoals;

        /// <summary> The potential actions this agent has access to. </summary>
        public HashSet<AgentAction> AvailableActions;


        /// <summary> A reference to the player's controller. </summary>
        private PlayerController _playerController;

        /// <summary> A reference to the planner this controller will use. </summary>
        private AgentPlanner _planner;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
            _planner = new AgentPlanner();

            InitialiseBeliefs();
            InitialiseActions();
            InitialiseGoals();
        }


        /// <summary> Set's the agent's initial beliefs. </summary>
        private void InitialiseBeliefs()    // TODO - These should be pulled from a JSON or something.
        {
            _availableBeliefs = new Dictionary<string, AgentBelief>();   // Initialise a new set of beliefs by clearing the current.
            BeliefFactory factory = new BeliefFactory(_controlledEntity, _availableBeliefs);

            factory.AddBelief("Nothing", () => false);  // Always has a belief, even if it never will successfully evaluate.

            factory.AddBelief("AgentIsIdle", () => _controlledEntity.NavigationAgent.IsNavigationFinished());
            factory.AddBelief("AgentIsMoving", () => !_controlledEntity.NavigationAgent.IsNavigationFinished());

            factory.AddBelief("AgentIsHealthy", () => _controlledEntity.Data.CurrentHealth >= 90f);
            factory.AddBelief("AgentIsHurt", () => _controlledEntity.Data.CurrentHealth < 50);
            factory.AddBelief("AgentIsFed", () => _controlledEntity.Data.CurrentHunger >= 90f);
            factory.AddBelief("AgentIsHungry", () => _controlledEntity.Data.CurrentHunger < 50f);
            factory.AddBelief("AgentIsRested", () => _controlledEntity.Data.CurrentFatigue >= 90f);
            factory.AddBelief("AgentIsTired", () => _controlledEntity.Data.CurrentFatigue < 50f);
            factory.AddBelief("AgentIsEntertained", () => _controlledEntity.Data.CurrentEntertainment >= 90f);
            factory.AddBelief("AgentIsBored", () => _controlledEntity.Data.CurrentEntertainment < 50f);

            factory.AddBrainBelief("AgentKnowsPlayer", _controlledEntity.Brain, _playerController.PlayerUnit);
            factory.AddBrainBelief("AgentKnowsFood", _controlledEntity.Brain, typeof(Food));

            factory.AddBelief("AgentSeesPlayer", () => _controlledEntity.Brain.KnowsEntity(_playerController.PlayerUnit).IsVisible);
        }


        /// <summary> Set's the agent's initial actions. </summary>
        private void InitialiseActions()
        {
            AvailableActions = new HashSet<AgentAction>();   // Initialise a new set of actions by clearing the current.

            AvailableActions.Add(new AgentAction.Builder("Relax")
                .WithStrategy(new IdleActionStrategy(_controlledEntity, 5f))
                .AddOutcome(_availableBeliefs["Nothing"])
                .Build());

            AvailableActions.Add(new AgentAction.Builder("Wander Around")
                .WithStrategy(new WanderActionStrategy(_controlledEntity, 2f))
                .AddOutcome(_availableBeliefs["AgentIsMoving"])
                .Build());

            /*
            AvailableActions.Add(new AgentAction.Builder("Find Food")
                .WithStrategy(new FindActionStrategy(_controlledEntity, typeof(Food)))
                .AddOutcome(_availableBeliefs["AgentKnowsPlayer"])
                .Build());
*/
            AvailableActions.Add(new AgentAction.Builder("Locate Player")
                .WithStrategy(new LocateActionStrategy(_controlledEntity, PlayerController.Instance.PlayerUnit))
                .AddPrecondition(_availableBeliefs["AgentKnowsPlayer"])
                .AddOutcome(_availableBeliefs["AgentSeesPlayer"])
                .Build());

            AvailableActions.Add(new AgentAction.Builder("Stalk Player")
                .WithStrategy(new StalkActionStrategy(_controlledEntity, _playerController.PlayerUnit))
                .AddPrecondition(_availableBeliefs["AgentSeesPlayer"])
                .AddPrecondition(_availableBeliefs["AgentIsBored"])
                .AddOutcome(_availableBeliefs["AgentIsEntertained"])
                .Build());
        }


        /// <summary> Set's the agent's initial goals. </summary>
        private void InitialiseGoals()
        {
            _availableGoals = new HashSet<AgentGoal>();


            _availableGoals.Add(new AgentGoal.Builder("Chill Out")
                .WithPriority(0)
                .WithDesiredOutcome(_availableBeliefs["Nothing"])
                .Build());

            _availableGoals.Add(new AgentGoal.Builder("Wander")
                .WithPriority(0)
                .WithDesiredOutcome(_availableBeliefs["AgentIsMoving"])
                .Build());

            _availableGoals.Add(new AgentGoal.Builder("Find Entertainment")
                .WithPriority(1)
                .WithDesiredOutcome(_availableBeliefs["AgentIsEntertained"])
                .Build());

            /*
            _availableGoals.Add(new AgentGoal.Builder("KeepHealthUp")
                .WithPriority(2)
                .WithDesiredOutcome(_availableBeliefs["AgentIsHealthy"])
                .Build());*/
        }


        /// <summary> Force a hard reset of the current plan. </summary>
        public void ReevaluatePlan()
        {
            // Remove the current objective to force the planner to reevaluate.
            _currentAction = null;
            ArchiveCurrentGoal();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Update the plan and current action if there is one
            if (_currentAction == null)
            {
                GD.Print("Calculating any potential new plan.");
                CalculatePlan();

                if (_currentPlan != null && _currentPlan.Actions.Count > 0)
                {
                    _currentGoal = _currentPlan.AgentGoal;
                    GD.Print($"Goal: {_currentGoal.Name} with {_currentPlan.Actions.Count} actions in plan");

                    _currentAction = _currentPlan.Actions.Pop();
                    GD.Print($"Popped action: {_currentAction.Name}");

                    // Verify all precondition effects are true
                    if (_currentAction.Preconditions.All(b => b.Evaluate()))
                    {
                        _currentAction.Start();
                    }
                    else
                    {
                        GD.Print("Preconditions not met, clearing current action and goal");

                        _currentAction = null;
                        _currentGoal = null;
                    }
                }
            }


            // If we have a current action, execute it
            if (_currentPlan != null && _currentAction != null)
            {
                _currentAction.Update(delta);

                if (_currentAction.IsComplete)
                {
                    GD.Print($"{_currentAction.Name} complete");

                    _currentAction.Stop();
                    _currentAction = null;

                    if (_currentPlan.Actions.Count == 0)
                    {
                        GD.Print("Plan complete");

                        ArchiveCurrentGoal();
                    }
                }
            }
        }


        /// <summary> Attempt to calculate a new plan. </summary>
        private void CalculatePlan()
        {
            Single priorityLevel = _currentGoal?.Priority ?? 0;

            HashSet<AgentGoal> goalsToCheck = _availableGoals;

            // If we have a current goal, we only want to check goals with higher priority.
            if (_currentGoal != null)
            {
                goalsToCheck = new HashSet<AgentGoal>(_availableGoals.Where(g => g.Priority > priorityLevel));
            }

            ActionPlan? potentialPlan = _planner.BuildPlan(this, goalsToCheck, _previousGoals[0]);
            if (potentialPlan != null)
            {
                _currentPlan = potentialPlan;
            }
        }


        /// <summary> Adds the current goal to the array of previous goals. </summary>
        private void ArchiveCurrentGoal()
        {
            AgentGoal[] newValues = new AgentGoal[10];
            newValues[0] = _currentGoal;
            Array.Copy(_previousGoals, 0, newValues, 1, _previousGoals.Length - 1);
            _previousGoals = newValues;

            _currentGoal = null;
        }
    }
}
