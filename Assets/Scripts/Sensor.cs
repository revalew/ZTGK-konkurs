using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

public static class GameObjectExtensions
{
  /// Returns the object itself if it exists, null otherwise.
  public static T OrNull<T>(this T obj) where T : UnityEngine.Object => obj ? obj : null;
}

[RequireComponent(typeof(SphereCollider))] // every sensor has to have a collider of some sort
public class Sensor : MonoBehaviour
{
  [SerializeField] float detectionRadius = 5.0f; // radius of the sensor
  [SerializeField] float timerInterval = 1.0f; // how often to check the sensor (instead of every frame)

  SphereCollider detectionRange;

  public event Action OnTargetChanged = delegate { };

  public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
  public bool IsTargetInRange => TargetPosition != Vector3.zero;

  GameObject target;
  Vector3 lastKnownPosition;
  CountdownTimer timer;

  void Awake()
  {
    detectionRange = GetComponent<SphereCollider>();
    detectionRange.isTrigger = true;
    detectionRange.radius = detectionRadius;
  }

  void Start()
  {
    timer = new CountdownTimer(timerInterval);
    timer.OnTimerStop += () =>
    {
      UpdateTargetPosition(target.OrNull());
      timer.Start();
    };
    timer.Start();
  }

  private void Update()
  {
    timer.Tick(Time.deltaTime);
  }

  void UpdateTargetPosition(GameObject target = null)
  {
    this.target = target;
    if (IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
    {
      lastKnownPosition = TargetPosition;
      OnTargetChanged.Invoke(); // target moved or new target
    }
  }

  //trigger collider for the player (JUST the player)
  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) { return; }
    UpdateTargetPosition(other.gameObject);
  }
  
  //trigger collider for the player (JUST the player)
  void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player")) { return; }
    UpdateTargetPosition();
  }

  private void OnDrawGizmos() // visible when target is in range
  {
    Gizmos.color = IsTargetInRange ? Color.red : Color.green;
    Gizmos.DrawWireSphere(transform.position, detectionRadius);
  }
}
