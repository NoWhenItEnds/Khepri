using Godot;
using System;
using System.Collections.Generic;

namespace Khepri.Models.GOAP
{
    public partial class AgentPlanner
    {
    }


    public class ActionPlan
    {
        public AgentGoal AgentGoal { get; }
        public Stack<AgentAction> Actions { get; }
        public Single TotalCost { get; set; }

        public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, Single totalCost)
        {
            AgentGoal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
    }
}
