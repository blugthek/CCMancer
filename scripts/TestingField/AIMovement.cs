#pragma warning disable 0414, 169, 414
/*
using UnityEngine;
using System.Collections.Generic;
using System;

public class AIMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float wanderStrength = 0.1f;
    public float avoidDistance = 2f;
    public float avoidStrength = 10f;
    public float wanderLerpFactor = 0.1f;

    private Vector2 currentWanderForce = Vector2.zero;
    private Vector2 target;
    private Rigidbody2D rb;
    private List<Transform> targets = new List<Transform>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        targets.Add(Instantiate(new GameObject(), new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)), Quaternion.identity).transform);
    }

    void Update()
    {
        if (targets.Count > 1)
        {
            Transform nearestTarget = FindNearestTarget();
            if (nearestTarget != null)
            {
                target = nearestTarget.position;
                MoveToTarget(target);
            }
            else
            {
                Wander();
            }
        }
        else
        {
            Wander();
        }

        AvoidThreats();
        AvoidBorders();
    }

    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        Vector2 currentPos = transform.position;
        foreach (Transform t in targets)
        {
            float dist = Vector2.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                nearest = t;
                minDist = dist;
            }
        }
        return nearest;
    }

    void MoveToTarget(Vector2 target)
    {
        Vector2 moveDirection = (target - (Vector2)transform.position).normalized;
        rb.velocity = moveDirection * moveSpeed;
    }

    void Wander()
    {
        Vector2 newWanderForce = new Vector2(Random.Range(-wanderStrength, wanderStrength), Random.Range(-wanderStrength, wanderStrength));

        currentWanderForce = Vector2.Lerp(currentWanderForce, newWanderForce, wanderLerpFactor);

        rb.velocity += currentWanderForce * Time.deltaTime;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, moveSpeed);
        
        Vector3 averagePosition = Vector3.zero;
        foreach (Transform t in targets)
        {
            averagePosition += t.position;
        }
        if (targets.Count > 0)
        {
            averagePosition /= targets.Count;
            Vector2 boidsForce = (Vector2)(averagePosition - transform.position).normalized * wanderStrength;
            rb.velocity += Vector2.Lerp(Vector2.zero, boidsForce, wanderLerpFactor) * Time.deltaTime;
        }
    }

    void AvoidThreats()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, avoidDistance);
        foreach (Collider2D hit in colliders)
        {
            if (hit.gameObject != gameObject)
            {
                Vector2 avoidDirection = (Vector2)(transform.position - hit.transform.position).normalized;
                rb.velocity += avoidDirection * avoidStrength * Time.deltaTime;
            }
        }
    }

    void AvoidBorders()
    {
        if (mainCamera == null) return;

        Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);

        float buffer = 0.05f;

        Vector2 steer = Vector2.zero;

        if (screenPos.x < buffer) steer.x = (buffer - screenPos.x) * borderAvoidStrength;
        else if (screenPos.x > 1 - buffer) steer.x = ((1 - buffer) - screenPos.x) * borderAvoidStrength;

        if (screenPos.y < buffer) steer.y = (buffer - screenPos.y) * borderAvoidStrength;
        else if (screenPos.y > 1 - buffer) steer.y = ((1 - buffer) - screenPos.y) * borderAvoidStrength;

        rb.velocity += steer * Time.deltaTime;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubscribeToTargetAdded(this);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnsubscribeFromTargetAdded(this);
        }
    }

    public void AddTarget(Transform newTarget)
    {
        if (!targets.Contains(newTarget))
        {
            targets.Add(newTarget);
        }
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private Dictionary<AIMovement, Action<Transform>> subscribers = new Dictionary<AIMovement, Action<Transform>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SubscribeToTargetAdded(AIMovement ai)
    {
        if (!subscribers.ContainsKey(ai))
        {
            subscribers[ai] = new Action<Transform>(ai.AddTarget);
        }
    }

    public void UnsubscribeFromTargetAdded(AIMovement ai)
    {
        if (subscribers.ContainsKey(ai))
        {
            subscribers.Remove(ai);
        }
    }

    private int targetCount = 0;

    public void NotifyTargetAdded(Transform target)
    {
        foreach (var subscriber in subscribers.Values)
        {
            subscriber?.Invoke(target);
        }
        targetCount++;
    }

    public int GetTargetCount()
    {
        return targetCount;
    }

    public void NotifyTargetRemoved()
    {
        targetCount = Mathf.Max(0, targetCount - 1);
    }
}

public class TargetManager : MonoBehaviour
{
    void CreateNewTarget()
    {
        Transform newTarget = Instantiate(new GameObject(), new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)), Quaternion.identity).transform;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyTargetAdded(newTarget);
        }
    }
}

public class Target : MonoBehaviour
{
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyTargetRemoved();
        }
    }
}

// 
// 

public class SpawnTarget : MonoBehaviour
{
    public GameObject targetPrefab;
    public float spawnInterval = 5f; 
    public int maxTargets = 5; 

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && (GameManager.Instance != null && GameManager.Instance.GetTargetCount() < maxTargets))
        {
            SpawnNewTarget();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnNewTarget()
    {
        if (targetPrefab != null)
        {
            
            Vector3 spawnPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), 0f);
            GameObject newTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyTargetAdded(newTarget.transform);
            }
        }
        else
        {
            Debug.LogError("Target prefab not assigned in SpawnTarget!");
        }
    }
}

*/