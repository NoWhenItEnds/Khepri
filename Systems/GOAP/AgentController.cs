using Godot;
using Khepri.Controllers;
using Khepri.Entities.Actors;
using Khepri.GOAP.ActionStrategies;
using Khepri.Resources;
using Khepri.Resources.Actors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.GOAP
{
    /// <summary> Uses a GOAP implementation to control an entity. The AI brain that controls a unit. </summary>
    public partial class AgentController : Node
    {
        /// <summary> The entity this controller is responsible for controlling. </summary>
        [ExportGroup("Nodes")]
        [Export] private Being _controlledEntity;


        /// <summary> The current goal the agent is trying to accomplish. </summary>
        public AgentGoal? CurrentGoal { get; private set; } = null;

        /// <summary> The current plan the agent is using to address its current goal. </summary>
        public ActionPlan? CurrentPlan { get; private set; } = null;

        /// <summary> The current action the agent is in the process of doing. </summary>
        public AgentAction? CurrentAction { get; private set; } = null;

        /// <summary> An ordered array of the previous goals the agent tried to accomplish. </summary>
        /// <remarks> [0] is the latest. [^1] is the oldest. </remarks>
        public AgentGoal[] PreviousGoals { get; private set; } = new AgentGoal[10];

        /// <summary> The 'truths' the agent knows. The beliefs it has about the world state. </summary>
        public Dictionary<String, AgentBelief> AvailableBeliefs { get; private set; }

        /// <summary> The goals that the agent will seek to address. </summary>
        public HashSet<AgentGoal> AvailableGoals { get; private set; }

        /// <summary> The potential actions this agent has access to. </summary>
        public HashSet<AgentAction> AvailableActions { get; private set; }


        /// <summary> A reference to the player's controller. </summary>
        private PlayerController _playerController;

        /// <summary> A reference to the game world's ui. </summary>
        private UIController _uiController;

        /// <summary> A reference to the planner this controller will use. </summary>
        private AgentPlanner _planner;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _playerController = PlayerController.Instance;
            _uiController = UIController.Instance;
            _planner = new AgentPlanner();

            BeingResource? agentResource = ResourceController.Instance.CreateResource<BeingResource>("human");
            if (agentResource == null)
            {
                throw new ArgumentNullException("The returned BeingResource is undefined. For some reason, you are missing a BeingResource with the 'human' id.");
            }
            _controlledEntity.Initialise(agentResource, _controlledEntity.GlobalPosition);

            InitialiseBeliefs();
            InitialiseActions();
            InitialiseGoals();
        }


        /// <summary> Set's the agent's initial beliefs. </summary>
        private void InitialiseBeliefs()    // TODO - These should be pulled from a JSON or something.
        {
            AvailableBeliefs = new Dictionary<string, AgentBelief>();   // Initialise a new set of beliefs by clearing the current.
            BeliefFactory factory = new BeliefFactory(_controlledEntity, AvailableBeliefs);

            factory.AddBelief("Nothing", () => false);  // Always has a belief, even if it never will successfully evaluate.

            factory.AddBelief("IsIdle", () => _controlledEntity.NavigationAgent.IsNavigationFinished());
            factory.AddBelief("IsMoving", () => !_controlledEntity.NavigationAgent.IsNavigationFinished());

            BeingNeedsResource needs = _controlledEntity.GetResource<BeingResource>().Needs;
            factory.AddBelief("IsHealthy", () => needs.CurrentHealth >= 90f);
            factory.AddBelief("IsHurt", () => needs.CurrentHealth < 50);
            factory.AddBelief("IsFed", () => needs.CurrentHunger >= 90f);
            factory.AddBelief("IsHungry", () => needs.CurrentHunger < 50f);
            factory.AddBelief("IsRested", () => needs.CurrentFatigue >= 90f);
            factory.AddBelief("IsTired", () => needs.CurrentFatigue < 50f);
            factory.AddBelief("IsEntertained", () => needs.CurrentEntertainment >= 90f);
            factory.AddBelief("IsBored", () => needs.CurrentEntertainment < 50f);

            // TODO - Add belief packages. Such as food beliefs that contains both the Knows and Sees for the item.
            factory.AddKnownItemBelief("KnowsApple", "apple");
            factory.AddItemLocationBelief("AtApple", "apple", 1f); // TODO - This should be based upon something.
            factory.AddInventoryBelief("HasApple", "apple");

            //factory.AddSensorBelief("AgentKnowsPlayer", _controlledEntity.Sensors, _playerController.PlayerUnit);
            //factory.AddBelief("AgentSeesPlayer", () => _controlledEntity.Sensors.TryGetEntity(_playerController.PlayerUnit).IsVisible);
        }


        /// <summary> Set's the agent's initial actions. </summary>
        private void InitialiseActions()
        {
            AvailableActions = new HashSet<AgentAction>();   // Initialise a new set of actions by clearing the current.

            AvailableActions.Add(new AgentAction.Builder("Relax")
                .WithStrategy(new IdleActionStrategy(_controlledEntity, 5f))
                .AddOutcome(AvailableBeliefs["Nothing"])
                .Build());

            AvailableActions.Add(new AgentAction.Builder("WanderAround")
                .WithStrategy(new WanderActionStrategy(_controlledEntity, 2f))
                .AddOutcome(AvailableBeliefs["IsMoving"])
                .Build());

            AvailableActions.Add(new AgentAction.Builder("GoToApple")   // TODO - Have harvest apple with a higher cost.
                .WithStrategy(new GoToItemActionStrategy(_controlledEntity, "apple"))
                .WithCost(10)
                .AddPrecondition(AvailableBeliefs["KnowsApple"])
                .AddOutcome(AvailableBeliefs["AtApple"])
                .Build());

            AvailableActions.Add(new AgentAction.Builder("PickupApple")
                .WithStrategy(new PickupActionStrategy(_controlledEntity, "apple"))
                .WithCost(0)
                .AddPrecondition(AvailableBeliefs["AtApple"])
                .AddOutcome(AvailableBeliefs["HasApple"])
                .Build());

            AvailableActions.Add(new AgentAction.Builder("EatApple")
                .WithStrategy(new UseInventoryItemActionStrategy(_controlledEntity, "apple"))
                .WithCost(5)    // This is how which items the agent prefers is encoded. Favorite items have a lower cost.
                .AddPrecondition(AvailableBeliefs["HasApple"])
                .AddOutcome(AvailableBeliefs["IsFed"])
                .Build());
        }


        /// <summary> Set's the agent's initial goals. </summary>
        private void InitialiseGoals()
        {
            AvailableGoals = new HashSet<AgentGoal>();


            AvailableGoals.Add(new AgentGoal.Builder("ChillOut")
                .WithPriority(0)
                .WithDesiredOutcome(AvailableBeliefs["Nothing"])
                .Build());

            AvailableGoals.Add(new AgentGoal.Builder("Wander")
                .WithPriority(0)
                .WithDesiredOutcome(AvailableBeliefs["IsMoving"])
                .Build());


            AvailableGoals.Add(new AgentGoal.Builder("KeepFed")
                .WithPriority(10)
                .WithDesiredOutcome(AvailableBeliefs["IsFed"])
                .Build());
        }


        /// <summary> Force a hard reset of the current plan. </summary>
        public void ReevaluatePlan()
        {
            // Remove the current objective to force the planner to reevaluate.
            CurrentAction = null;
            ArchiveCurrentGoal();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            // Update the plan and current action if there is one
            if (CurrentAction == null)
            {
                _uiController.SpawnSpeechBubble("Calculating any potential new plan.", _controlledEntity);
                CalculatePlan();

                if (CurrentPlan != null && CurrentPlan.Actions.Count > 0)
                {
                    CurrentGoal = CurrentPlan.AgentGoal;
                    _uiController.SpawnSpeechBubble($"Goal: {CurrentGoal.Name} with {CurrentPlan.Actions.Count} actions in plan", _controlledEntity);

                    CurrentAction = CurrentPlan.Actions.Pop();
                    _uiController.SpawnSpeechBubble($"Popped action: {CurrentAction.Name}", _controlledEntity);

                    // Verify all precondition effects are true
                    if (CurrentAction.Preconditions.All(b => b.Evaluate()))
                    {
                        CurrentAction.Start();
                    }
                    else
                    {
                        _uiController.SpawnSpeechBubble("Preconditions not met, clearing current action and goal", _controlledEntity);

                        CurrentAction = null;
                        CurrentGoal = null;
                    }
                }
            }


            // If we have a current action, execute it
            if (CurrentPlan != null && CurrentAction != null)
            {
                CurrentAction.Update(delta);

                if (CurrentAction.IsComplete)
                {
                    _uiController.SpawnSpeechBubble($"Action, {CurrentAction.Name}, complete.", _controlledEntity);

                    CurrentAction.Stop();
                    CurrentAction = null;

                    if (CurrentPlan.Actions.Count == 0)
                    {
                        _uiController.SpawnSpeechBubble("Plan complete!", _controlledEntity);

                        ArchiveCurrentGoal();
                    }
                }
            }
        }


        /// <summary> Attempt to calculate a new plan. </summary>
        private void CalculatePlan()
        {
            Single priorityLevel = CurrentGoal?.Priority ?? 0;

            HashSet<AgentGoal> goalsToCheck = AvailableGoals;

            // If we have a current goal, we only want to check goals with higher priority.
            if (CurrentGoal != null)
            {
                goalsToCheck = new HashSet<AgentGoal>(AvailableGoals.Where(g => g.Priority > priorityLevel));
            }

            ActionPlan? potentialPlan = _planner.BuildPlan(this, goalsToCheck, PreviousGoals[0]);
            if (potentialPlan != null)
            {
                CurrentPlan = potentialPlan;
            }
        }


        /// <summary> Adds the current goal to the array of previous goals. </summary>
        private void ArchiveCurrentGoal()
        {
            AgentGoal[] newValues = new AgentGoal[10];
            newValues[0] = CurrentGoal;
            Array.Copy(PreviousGoals, 0, newValues, 1, PreviousGoals.Length - 1);
            PreviousGoals = newValues;

            CurrentGoal = null;
        }
    }
}
