using System;
using System.Collections.Generic;
using System.Linq;

namespace Khepri.Models.GOAP
{
    /// <summary> Handles the logic of constructing a method to reach a desired goal. </summary>
    /// <remarks> https://www.youtube.com/watch?v=T_sBYgP7_2k </remarks>
    public partial class AgentPlanner
    {
        /// <summary> Attempt to build a plan to address the agent's highest priority goal. </summary>
        /// <param name="agent"> A reference to the agent for whom this plan is for. </param>
        /// <param name="goals"> The goals to plan for. </param>
        /// <param name="mostRecentGoal"> A reference to the most recent goal the agent attempted to address. </param>
        /// <returns> The constructed plan. A null value means that we couldn't find one. </returns>
        public ActionPlan? BuildPlan(AgentController agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
        {
            // Order goals by priority, descending
            List<AgentGoal> orderedGoals = goals
                //.Where(g => g.DesiredOutcomes.Any(b => !b.Evaluate()))  // Don't include goals who's outcomes are already complete.
                .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01 : g.Priority)   // Don't keep trying to get the same goal (the most recent one) all the time. Give it a sightly lower priority.
                .ToList();

            // Try to solve each goal in order
            foreach (AgentGoal goal in orderedGoals)
            {
                GraphNode goalNode = new GraphNode(null, null, goal.DesiredOutcomes, 0);

                // If we can find a path to the goal, return the plan
                if (FindPath(goalNode, agent.AvailableActions))
                {
                    // If the goalNode has no leaves and no action to perform try a different goal
                    if (!goalNode.IsLeafDead)
                    {
                        Stack<AgentAction> actionStack = new Stack<AgentAction>();
                        while (goalNode.Leaves.Count > 0)
                        {
                            GraphNode cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                            goalNode = cheapestLeaf;
                            actionStack.Push(cheapestLeaf.Action);
                        }

                        return new ActionPlan(goal, actionStack, goalNode.Cost);
                    }
                }
            }

            return null;
        }


        /// <summary> Continue tracing a path from the parent using the available actions. </summary>
        /// <param name="parent"> A reference to the direction parent node we're pathing from. </param>
        /// <param name="actions"> The set of actions we have access to at this step. </param>
        /// <returns> Whether a path was successfully found at this level. </returns>
        private Boolean FindPath(GraphNode parent, HashSet<AgentAction> actions)
        {
            // Order actions by cost, ascending
            IOrderedEnumerable<AgentAction> orderedActions = actions.OrderBy(a => a.Cost);

            foreach (AgentAction action in orderedActions)
            {
                HashSet<AgentBelief> requiredBeliefs = parent.RequiredBeliefs;

                // Remove any beliefs that evaluate to true, there is no action to take. They're already done.
                requiredBeliefs.RemoveWhere(b => b.Evaluate());

                // If there are no required beliefs to fulfill, we have a plan. No need to search further.
                if (requiredBeliefs.Count == 0)
                {
                    return true;
                }

                // If this action addresses any of the required outcomes, it's worth exploring.
                if (action.Outcomes.Any(requiredBeliefs.Contains))
                {
                    HashSet<AgentBelief> newRequiredBeliefs = new HashSet<AgentBelief>(requiredBeliefs);
                    newRequiredBeliefs.ExceptWith(action.Outcomes); // Remove any that have already been satisfied.
                    newRequiredBeliefs.UnionWith(action.Preconditions); // Add any preconditions that haven't been.

                    GraphNode newNode = new GraphNode(parent, action, newRequiredBeliefs, parent.Cost + action.Cost);

                    // Explore the new node, recursively.
                    if (FindPath(newNode, actions))
                    {
                        parent.Leaves.Add(newNode);
                        newRequiredBeliefs.ExceptWith(newNode.Action.Preconditions);
                    }

                    // If all effects at this depth have been satisfied, return true
                    if (newRequiredBeliefs.Count == 0)
                    {
                        return true;
                    }
                }
            }

            return parent.Leaves.Count > 0;
        }
    }


    /// <summary> A data structure for holding a desired goal and the actions needed to reach the goal. </summary>
    public class ActionPlan
    {
        /// <summary> The goal this plan is attempting to satisfy. </summary>
        public AgentGoal AgentGoal { get; }

        /// <summary> The ordered actions required to satisfy the goal. </summary>
        public Stack<AgentAction> Actions { get; }

        /// <summary> The plan's total cost. A sum of all the actions' costs. </summary>
        public Single TotalCost { get; set; }


        /// <summary> A data structure for holding a desired goal and the actions needed to reach the goal. </summary>
        /// <param name="goal"> The goal this plan is attempting to satisfy. </param>
        /// <param name="actions"> The ordered actions required to satisfy the goal. </param>
        /// <param name="totalCost"> The plan's total cost. A sum of all the actions' costs. </param>
        public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, Single totalCost)
        {
            AgentGoal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
    }


    /// <summary> A node in a graph data structure. </summary>
    public class GraphNode
    {
        /// <summary> A reference to this node's parent. </summary>
        public GraphNode Parent { get; }

        /// <summary> The action this node represents. </summary>
        public AgentAction Action { get; }

        /// <summary> All the beliefs at THIS position in the graph. </summary>
        public HashSet<AgentBelief> RequiredBeliefs { get; }

        /// <summary> References to all the children leaves. </summary>
        public List<GraphNode> Leaves { get; }

        /// <summary> A running cost of how expensive the graph is at this point. </summary>
        public Single Cost { get; }

        /// <summary> A node isn't worth considering if it has no children and no associated action. </summary>
        public Boolean IsLeafDead => Leaves.Count == 0 && Action == null;


        /// <summary> A node in a graph data structure. </summary>
        /// <param name="parent"> A reference to this node's parent. </param>
        /// <param name="action"> The action this node represents. </param>
        /// <param name="beliefs"> All the beliefs at THIS position in the graph. </param>
        /// <param name="cost"> A running cost of how expensive the graph is at this point. </param>
        public GraphNode(GraphNode parent, AgentAction action, HashSet<AgentBelief> beliefs, Single cost)
        {
            Parent = parent;
            Action = action;
            RequiredBeliefs = new HashSet<AgentBelief>(beliefs);    // We make a new set as there may be additional beliefs we need to satisfy as a result of our path.
            Leaves = new List<GraphNode>();
            Cost = cost;
        }
    }
}
