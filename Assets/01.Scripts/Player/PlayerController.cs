using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Logger = Common.Logger;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 10.0f;
    
    [Header("Idle Settings")]
    [Range(0.0f, 10.0f)]
    [SerializeField] private float minIdleTime = 0.0f;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float maxIdleTime = 10.0f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private List<Transform> patrolPointList = new List<Transform>();
    private List<Transform> wayPointList = new List<Transform>();
    private List<int> wayPointIdxList = new List<int>();
    private Transform targetWayPoint;
    private int wayPointIdx;
    
    private float idleTime;
    private float elapsedTime;
    
    private Animator animator;
    private NavMeshAgent agent;

    private IPatrol curState;
    private Coroutine updateRoutine;
    private bool isInitialized = false;
    
    public bool IsMoving => agent != null && agent.velocity.magnitude > 0.1f;
    public bool IsIdle => curState is PatrolIdle;
    public Transform CurrentTarget => targetWayPoint;

    public event Action<Vector3> OnDestinationReached;
    public event Action<IPatrol> OnStateChanged;

    private void Awake()
    {
        try
        {
            InitializeComponents();
            InitializePatrolPoints();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in Awake");
        }
        
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        patrolPointList = GameObject.Find("PatrolPoint")?.GetComponentsInChildren<Transform>().ToList();
        patrolPointList?.RemoveAt(0);
    }

    private void Start()
    {
        try
        {
            if (ValidateSetup())
            {
                GenerateWaypoints();
                SetInitialPosition();
                isInitialized = true;
                ChangeState<PatrolIdle>();

                if (debugMode)
                {
                    Logger.Log($"{GetType()}:: {patrolPointList.Count} PatrolPoint is found");
                }
            }
            else
            {
                Logger.LogError($"{GetType()}:: Setup is not valid");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in Start");
        }
    }

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Logger.LogError($"{GetType()}:: Animator is not found");
        }
        
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Logger.LogError($"{GetType()}:: NavMeshAgent is not found");
            return;
        }
        
        agent.updateRotation = false;
        agent.speed = moveSpeed;
    }
    
    private void InitializePatrolPoints()
    {
        var patrolParent = GameObject.Find("PatrolPoint");
        if (patrolParent != null)
        {
            patrolPointList = patrolParent.GetComponentsInChildren<Transform>().ToList();
            patrolPointList?.RemoveAt(0);

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: {patrolPointList?.Count} PatrolPoint is found");
            }
            else
            {
                Logger.LogWarning($"{GetType()}:: {patrolPointList?.Count} PatrolPoint is found");   
            }
        }
    }

    private bool ValidateSetup()
    {
        if (agent == null)
        {
            Logger.LogError($"{GetType()}:: NavMeshAgent is not found");
            return false;
        }

        if (patrolPointList == null || patrolPointList.Count.Equals(0))
        {
            Logger.LogWarning($"{GetType()}:: PatrolPoint is not found");
            return false;
        }

        if (minIdleTime > maxIdleTime)
        {
            Logger.LogWarning($"{GetType()}:: MinIdleTime is greater than MaxIdleTime");
            (minIdleTime, maxIdleTime) = (maxIdleTime, minIdleTime);
        }

        return true;
    }
    
    private void GenerateWaypoints()
    {
        if (patrolPointList == null || patrolPointList.Count.Equals(0))
        {
            Logger.LogError($"{GetType()}:: PatrolPoint is not found");
            return;
        }

        try
        {
            wayPointIdxList.Clear();
            var availableIndices = Enumerable.Range(0, patrolPointList.Count).ToList();

            for (int i = 0; i < availableIndices.Count; i++)
            {
                var randomIndex = Random.Range(i, availableIndices.Count);
                (availableIndices[i], availableIndices[randomIndex]) = (availableIndices[randomIndex], availableIndices[i]);
            }
            
            wayPointIdxList.AddRange(availableIndices);

            wayPointList.Clear();
            foreach (int idx in wayPointIdxList)
            {
                if (idx < patrolPointList.Count && patrolPointList[idx] != null)
                {
                    wayPointList.Add(patrolPointList[idx]);
                }
            }

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: {wayPointList.Count} WayPoint is generated");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in GenerateWaypoints");
        }
    }
    
    private void SetInitialPosition()
    {
        if (wayPointList.Count > 0 && wayPointList[0] != null)
        {
            try
            {
                transform.position = wayPointList[0].position;
                wayPointIdx = 0;
                
                if (debugMode)
                {
                    Logger.Log($"{GetType()}:: Initial Position is set");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{GetType()}:: Error in SetInitialPosition");
            }
        }
    }

    public T ChangeState<T>() where T : Component, IPatrol
    {
        if (!isInitialized)
        {
            Logger.LogWarning($"{GetType()}:: PlayerController is not initialized");
            return null;
        }

        if (curState is T)
        {
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: State is already {typeof(T).Name}");
            }
            return (T) curState;
        }

        try
        {
            curState?.OnEnd();
            StopUpdateLoop();
            DestroyCurrentState();

            curState = AddState<T>();
            if (curState != null)
            {
                curState.OnStart();
                elapsedTime = 0f;
                updateRoutine = StartCoroutine(StartUpdateLoop());
                
                OnStateChanged?.Invoke(curState);
                
                if (debugMode)
                {
                    Logger.Log($"{GetType()}:: State is changed to {typeof(T).Name}");
                }
            }
            
            return curState as T;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in ChangeState");
            return null;
        }
    }

    private IEnumerator StartUpdateLoop()
    {
        while (curState != null && isInitialized)
        {
            try
            {
                elapsedTime += Time.deltaTime;
                curState?.OnUpdate(elapsedTime);
                
                if (agent != null && agent.remainingDistance > 0.1f && agent.desiredVelocity != Vector3.zero)
                {
                    var targetRotation = Quaternion.LookRotation(agent.desiredVelocity);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime * turnSpeed);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{GetType()}:: Error in StartUpdateLoop");
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void StopUpdateLoop()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }

    private void DestroyCurrentState()
    {
        if (curState is Component comp && comp != null)
        {
            try
            {
                Destroy(comp);
                curState = null;
            }
            catch (Exception e)
            {
                Logger.LogError($"{GetType()}:: Error in DestroyCurrentState");
            }

        }
    }

    private T AddState<T>() where T : Component
    {
        try
        {
            return gameObject.AddComponent<T>();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in AddState");
            return null;
        }
    }

    public Transform FindWayPoint()
    {
        if (wayPointList == null || wayPointList.Count.Equals(0))
        {
            Logger.LogWarning($"{GetType()}:: WayPoint is not found");
            return null;
        }

        try
        {
            targetWayPoint = wayPointList[wayPointIdx];
            wayPointIdx = (wayPointIdx + 1) % wayPointList.Count;
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Next WayPoint is {targetWayPoint.name}");
            }
            
            return targetWayPoint;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in FindWayPoint");
            return null;
        }
    }

    public bool SetDestination()
    {
        var nextWayPoint = FindWayPoint();
        if (nextWayPoint == null || agent == null)
        {
            return false;
        }

        try
        {
            agent.SetDestination(nextWayPoint.position);
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Destination is set to {nextWayPoint.name}");
            }
            
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in SetDestination");
            return false;
        }
    }

    public void Idle()
    {
        try
        {
            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", 0.0f);
            }

            if (agent != null)
            {
                agent.isStopped = true;
            }

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Idle");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in Idle");
        }
    }

    public void Move()
    {
        try
        {
            if (agent != null)
            {
                agent.isStopped = false;
            }

            if (animator != null)
            {
                animator.SetFloat("MoveSpeed", 0.4f);
            }
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Move");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in Move");
        }
    }

    public bool HasArrived()
    {
        if (agent == null)
        {
            return true;
        }

        try
        {
            if (agent.pathPending)
            {
                return false;
            }

            if (agent.remainingDistance > agent.stoppingDistance)
            {
                return false;
            }
            
            bool arrived = !agent.hasPath || agent.velocity.sqrMagnitude < 0.1f;
            
            if (arrived)
            {
                Logger.Log($"{GetType()}:: Arrived");
                OnDestinationReached?.Invoke(transform.position);
            }
            
            return arrived;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in HasArrived");
            return true;
        }
    }

    public bool HasFinishedIdle()
    {
        bool finished = elapsedTime > idleTime;
        if (finished && debugMode)
        {
            Logger.Log($"{GetType()}:: Idle is finished");
        }
        
        return finished;
    }

    public void ResetPath()
    {
        try
        {
            if (agent != null)
            {
                agent.ResetPath();
            }
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Path is reset");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in ResetPath");
        }
    }

    public void SetIdleTime()
    {
        try
        {
            idleTime = Random.Range(minIdleTime, maxIdleTime);

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: IdleTime is set to {idleTime}");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in SetIdleTime");
            idleTime = 2.0f;
        }
    }

    public void PauseMovement()
    {
        try
        {
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in PauseMovement");
        }
    }

    public void ResumeMovement()
    {
        try
        {
            if (agent != null && curState is PatrolMove)
            {
                agent.isStopped = false;
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in ResumeMovement");
        }
    }

    public void RegenerateWayPoints()
    {
        GenerateWaypoints();
        SetInitialPosition();
    }

    private void OnDestroy()
    {
        try
        {
            StopUpdateLoop();
            DestroyCurrentState();

            OnDestinationReached = null;
            OnStateChanged = null;

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Destroyed");
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}:: Error in OnDestroy {e.Message}");
        }
    }

    private void OnValidate()
    {
        if (minIdleTime < 0)
        {
            minIdleTime = 0;
        }

        if (maxIdleTime < minIdleTime)
        {
            maxIdleTime = minIdleTime;
        }

        if (moveSpeed < 0)
        {
            moveSpeed = 1.0f;
        }

        if (turnSpeed < 0)
        {
            turnSpeed = 1.0f;
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugMode)
        {
            return;
        }

        if (wayPointList != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < wayPointList.Count; i++)
            {
                if (wayPointList[i] != null)
                {
                    Gizmos.DrawWireSphere(wayPointList[i].position, 0.5f);
                    
                    int nextIndex = (i + 1) % wayPointList.Count;
                    if (wayPointList[nextIndex] != null)
                    {
                        Gizmos.DrawLine(wayPointList[i].position, wayPointList[nextIndex].position);
                    }
                }
            }
        }

        if (targetWayPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetWayPoint.position, 0.7f);
        }

        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            var path = agent.path.corners;
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
#endif
}
