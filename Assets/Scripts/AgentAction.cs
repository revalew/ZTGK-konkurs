using System.Collections.Generic;

public class AgentAction
{
  public string Name { get; }
  public float Cost { get; }

  public HashSet<AgentBeliefs> Preconditions{ get; } = new();
  public HashSet<AgentBeliefs> Effects{ get; } = new();

  IActionStrategy strategy;
  public bool Complete => strategy.Complete;

  public void Start() => strategy.Start();

  public void Update() => strategy.Update();

  public void Stop() => strategy.Stop();
}
