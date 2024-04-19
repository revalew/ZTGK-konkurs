using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// beliefs factory
public class BeliefFactory
{
  readonly GoapAgent agent;
  readonly Dictionary<string, AgentBeliefs> beliefs;

  public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBeliefs> beliefs)
  {
    this.agent = agent;
    this.beliefs = beliefs;
  }

  public void AddBelief(string key, Func<bool> condition)
  {
    beliefs.Add(key, new AgentBeliefs.Builder(key)
      .WithCondition(condition)
      .Build());
  }

  // Beliefs based on Sensor information
  public void AddSensorBelief(string key, Sensor sensor)
  {
    beliefs.Add(key, new AgentBeliefs.Builder(key)
      .WithCondition(() => sensor.IsTargetInRange)
      .WithLocation(() => sensor.TargetPosition)
      .Build());
  }

  //create beliefs based on the location of the in-game object
  public void AddLocationBelief(string key, float distance, Transform locationCondition)
  {
    AddLocationBelief(key, distance, locationCondition.position);
  }

  //create beliefs based on the coordinates of a point on the map
  public void AddLocationBelief(string key, float distance, Vector3 locationCondition)
  {
    beliefs.Add(key, new AgentBeliefs.Builder(key)
      .WithCondition(() => InRangeOf(locationCondition, distance))
      .WithLocation(() => locationCondition)
      .Build());
  }

  bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position, pos) < range;

}



public class AgentBeliefs
{
  public string Name { get; }

  Func<bool> condition = () => false;
  Func<Vector3> observedLocation = () => Vector3.zero;

  public Vector3 Location => observedLocation();

  AgentBeliefs(string name)
  {
    Name = name;
  }

  public bool Evaluate() => condition();

  public class Builder
  {
    readonly AgentBeliefs belief;

    public Builder(string name)
    {
      belief = new AgentBeliefs(name);
    }

    public Builder WithCondition(Func<bool> condition)
    {
      belief.condition = condition;
      return this;
    }

    public Builder WithLocation(Func<Vector3> observedLocation)
    {
      belief.observedLocation = observedLocation;
      return this;
    }

    public AgentBeliefs Build()
    {
      return belief;
    }
  }
}
