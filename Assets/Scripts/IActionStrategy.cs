using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy
{
  bool CanPerform { get; }
  bool Complete { get; }

  void Start()
  {
    //noop
  }

  void Update(float deltaTime)
  {
    //noop
  }

  void Stop()
  {
    //noop
  }
}

public class AttackStrategy : IActionStrategy
{
  public bool CanPerform => true; // Agent can always attack
  public bool Complete { get; private set; }

  readonly CountdownTimer timer;
  readonly AnimationController animations;

  public AttackStrategy(AnimationController animations)
  {
    this.animations = animations;
    timer = new CountdownTimer(animations.GetAnimationLength(animations.attackClip));
    timer.OnTimerStart += () => Complete = false;
    timer.OnTimerStop += () => Complete = true;
  }

  public void Start()
  {
    timer.Start();
    animations.Attack();
  }

  public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class MoveStrategy : IActionStrategy
{
  readonly NavMeshAgent agent;
  readonly Func<Vector3> destination;

  public bool CanPerform => !Complete;
  public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

  public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
  {
    this.agent = agent;
    this.destination = destination;
  }

  public void Start() => agent.SetDestination(destination());
  public void Stop() => agent.ResetPath();
}

public class WanderStrategy : IActionStrategy
{
  readonly NavMeshAgent agent;
  readonly float wanderRadius;

  public bool CanPerform => !Complete;
  public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

  public WanderStrategy(NavMeshAgent agent, float wanderRadius)
  {
    this.agent = agent;
    this.wanderRadius = wanderRadius;
  }

  public void Start()
  {
    for (int i = 0; i < 5; i++)
    {
      Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
      NavMeshHit hit;

      if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1))
      {
        agent.SetDestination(hit.position);
        return;
      }
    }
  }
}

public class IdleStrategy : IActionStrategy
{
  public bool CanPerform => true; // Agent can always Idle, nothing is gointt to stop us from doing that 
  public bool Complete { get; private set; }

  readonly CountdownTimer timer;

  public IdleStrategy(float duration)
  {
    timer = new CountdownTimer(duration);
    timer.OnTimerStart += () => Complete = false;
    timer.OnTimerStop += () => Complete = true;
  }

  public void Start() => timer.Start();
  public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public static class Vector3Extensions
{
  /// <summary>
  /// Sets any x y z values of a Vector3
  /// </summary>
  public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
  {
    return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
  }
}