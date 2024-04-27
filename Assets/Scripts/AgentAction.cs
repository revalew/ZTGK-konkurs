using System.Collections.Generic;

public class AgentAction
{
  public string Name { get; }
  public float Cost { get; private set; }

  public HashSet<AgentBeliefs> Preconditions{ get; } = new();
  public HashSet<AgentBeliefs> Effects{ get; } = new();

  IActionStrategy strategy;
  public bool Complete => strategy.Complete;


  AgentAction(string name)
  {
    Name = name;
  }


  public void Start() => strategy.Start();

  public void Update(float deltaTime)
  {
    // Check if the action can be performed and update the strategy
    if (strategy.CanPerform)
    {
      strategy.Update(deltaTime);
    }

    // Bail out if the strategy is still executing
    if (!strategy.Complete) return;

    // Aply effects => reevaluate the beliefs
    foreach (var effect in Effects)
    {
      effect.Evaluate();
    }
  }

  public void Stop() => strategy.Stop();

  public class Builder
  {
    readonly AgentAction action;

    public Builder(string name)
    {
      action = new AgentAction(name)
      {
        Cost = 1
      };
    }

    public Builder WithCost(float cost)
    {
      action.Cost = cost;
      return this;
    }

    public Builder WithStrategy(IActionStrategy strategy)
    {
      action.strategy = strategy;
      return this;
    }

    public Builder AddPrecondition(AgentBeliefs precondition)
    {
      action.Preconditions.Add(precondition);
      return this;
    }

    public Builder AddEffect(AgentBeliefs effect)
    {
      action.Effects.Add(effect);
      return this;
    }

    public AgentAction Build()
    {
      return action;
    }
  }
}
