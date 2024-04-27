using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IGoapPlanner
{ // good place to inject the mock plan for testing purposes
  ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class GoapPlanner : IGoapPlanner
{
  public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
  {
    // Order goals by priority (descending)
    List<AgentGoal> orderedGoals = goals
        .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
        .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01 : g.Priority)
        .ToList();

    // Try to solve each goal in order
    foreach (var goal in orderedGoals)
    {
      Node goalNode = new Node(null, null, goal.DesiredEffects, 0); // start with goal in mind (top priority)

      // If we can find a path to the goal, return the plan
      if (FindPath(goalNode, agent.actions))
      {
        // If the goalNode has no leaves and no action to perform try a different goal
        if (goalNode.IsLeafDead) continue;

        Stack<AgentAction> actionStack = new Stack<AgentAction>();
        while (goalNode.Leaves.Count > 0)
        {
          var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First(); // sort actions by cost
          goalNode = cheapestLeaf;
          actionStack.Push(cheapestLeaf.Action);
        }

        return new ActionPlan(goal, actionStack, goalNode.Cost);
      }
    }

    Debug.LogWarning("No plan found!");
    return null;
  }

  // TODO: Consider a more powerful search algorithm like A* or D* (instead of the DFS)
  bool FindPath(Node parent, HashSet<AgentAction> actions)
  {
    // Order actions by cost, ascending
    var orderedActions = actions.OrderBy(a => a.Cost);

    foreach (var action in orderedActions)
    {
      var requiredEffects = parent.RequiredEffects;

      // Remove any effects that evaluate to true => there is no action to take
      requiredEffects.RemoveWhere(b => b.Evaluate());

      // If there are no required effects to fulfill => we have a plan
      if (requiredEffects.Count == 0)
      {
        return true;
      }

      if (action.Effects.Any(requiredEffects.Contains))
      {
        var newRequiredEffects = new HashSet<AgentBeliefs>(requiredEffects);
        newRequiredEffects.ExceptWith(action.Effects);
        newRequiredEffects.UnionWith(action.Preconditions);

        var newAvailableActions = new HashSet<AgentAction>(actions);
        newAvailableActions.Remove(action); // actions cannot be used more than once per plan. With complicated plans / goals we might want to remove that (keep action in list)

        var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

        // Explore the new node recursively
        if (FindPath(newNode, newAvailableActions))
        {
          parent.Leaves.Add(newNode);
          newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
        }

        // all effects at this depth have been satisfied => return true
        if (newRequiredEffects.Count == 0)
        {
          return true;
        }
      }
    }

    return false;
  }
}

public class Node
{
  public Node Parent { get; }
  public AgentAction Action { get; }
  public HashSet<AgentBeliefs> RequiredEffects { get; }
  public List<Node> Leaves { get; }
  public float Cost { get; }

  public bool IsLeafDead => Leaves.Count == 0 && Action == null; // has no children / action

  public Node(Node parent, AgentAction action, HashSet<AgentBeliefs> effects, float cost)
  {
    Parent = parent;
    Action = action;
    RequiredEffects = new HashSet<AgentBeliefs>(effects);
    Leaves = new List<Node>();
    Cost = cost;
  }
}

public class ActionPlan
{
  public AgentGoal AgentGoal { get; }
  public Stack<AgentAction> Actions { get; }
  public float TotalCost { get; set; } // sum of costs of actions

  public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, float totalCost)
  {
    AgentGoal = goal;
    Actions = actions;
    TotalCost = totalCost;
  }
}