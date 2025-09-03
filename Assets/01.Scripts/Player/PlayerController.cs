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
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 10.0f;
    
    [Header("Idle Settings")]
    [Range(0.0f, 10.0f)]
    [SerializeField] private float minIdleTime;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float maxIdleTime = 10.0f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode;

    private List<Transform> _patrolPointList = new List<Transform>();
    [SerializeField] private List<Transform> wayPointList = new List<Transform>();
    private readonly List<int> _wayPointIdxList = new List<int>();
    private Transform _targetWayPoint;
    private int _wayPointIdx;
    
    private float _idleTime;
    private float _elapsedTime;
    
    private Animator _animator;
    private NavMeshAgent _agent;

    private IPatrol _curState;
    private Coroutine _updateRoutine;
    private bool _isInitialized;
    
    public bool IsMoving => _agent != null && _agent.velocity.magnitude > 0.1f;
    public bool IsIdle => _curState is PatrolIdle;
    public Transform CurrentTarget => _targetWayPoint;

    public event Action<Vector3> OnDestinationReached;
    public event Action<IPatrol> OnStateChanged;

    private void Awake()
    {
        try
        {
            InitializeComponents();
            InitializePatrolPoints();
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in Awake");
        }
        
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;

        _patrolPointList = GameObject.Find("PatrolPoint")?.GetComponentsInChildren<Transform>().ToList();
        _patrolPointList?.RemoveAt(0);
    }

    private void Start()
    {
        try
        {
            if (ValidateSetup())
            {
                GenerateWaypoints();
                SetInitialPosition();
                _isInitialized = true;
                ChangeState<PatrolIdle>();

                if (debugMode)
                {
                    Logger.Log($"{GetType()}:: {_patrolPointList.Count} PatrolPoint is found");
                }
            }
            else
            {
                Logger.LogError($"{GetType()}:: Setup is not valid");
            }
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in Start");
        }
    }

    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Logger.LogError($"{GetType()}:: Animator is not found");
        }
        
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Logger.LogError($"{GetType()}:: NavMeshAgent is not found");
            return;
        }
        
        _agent.updateRotation = false;
        _agent.speed = moveSpeed;
    }
    
    private void InitializePatrolPoints()
    {
        var patrolParent = GameObject.Find("PatrolPoint");
        if (patrolParent != null)
        {
            _patrolPointList = patrolParent.GetComponentsInChildren<Transform>().ToList();
            _patrolPointList?.RemoveAt(0);

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: {_patrolPointList?.Count} PatrolPoint is found");
            }
            else
            {
                Logger.LogWarning($"{GetType()}:: {_patrolPointList?.Count} PatrolPoint is found");   
            }
        }
    }

    private bool ValidateSetup()
    {
        if (_agent == null)
        {
            Logger.LogError($"{GetType()}:: NavMeshAgent is not found");
            return false;
        }

        if (_patrolPointList == null || _patrolPointList.Count.Equals(0))
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
        if (_patrolPointList == null || _patrolPointList.Count.Equals(0))
        {
            Logger.LogError($"{GetType()}:: PatrolPoint is not found");
            return;
        }

        try
        {
            _wayPointIdxList.Clear();
            var availableIndices = Enumerable.Range(0, _patrolPointList.Count).ToList();

            for (int i = 0; i < availableIndices.Count; i++)
            {
                var randomIndex = Random.Range(i, availableIndices.Count);
                (availableIndices[i], availableIndices[randomIndex]) = (availableIndices[randomIndex], availableIndices[i]);
            }
            
            _wayPointIdxList.AddRange(availableIndices);

            wayPointList.Clear();
            foreach (int idx in _wayPointIdxList)
            {
                if (idx < _patrolPointList.Count && _patrolPointList[idx] != null)
                {
                    wayPointList.Add(_patrolPointList[idx]);
                }
            }

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: {wayPointList.Count} WayPoint is generated");
            }
        }
        catch (Exception)
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
                _wayPointIdx = 0;
                
                if (debugMode)
                {
                    Logger.Log($"{GetType()}:: Initial Position is set");
                }
            }
            catch (Exception)
            {
                Logger.LogError($"{GetType()}:: Error in SetInitialPosition");
            }
        }
    }

    public T ChangeState<T>() where T : Component, IPatrol
    {
        if (!_isInitialized)
        {
            Logger.LogWarning($"{GetType()}:: PlayerController is not initialized");
            return null;
        }

        if (_curState is T state)
        {
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: State is already {typeof(T).Name}");
            }
            return state;
        }

        try
        {
            _curState?.OnEnd();
            StopUpdateLoop();
            DestroyCurrentState();

            _curState = AddState<T>();
            if (_curState != null)
            {
                _curState.OnStart();
                _elapsedTime = 0f;
                _updateRoutine = StartCoroutine(StartUpdateLoop());
                
                OnStateChanged?.Invoke(_curState);
                
                if (debugMode)
                {
                    Logger.Log($"{GetType()}:: State is changed to {typeof(T).Name}");
                }
            }
            
            return _curState as T;
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in ChangeState");
            return null;
        }
    }

    private IEnumerator StartUpdateLoop()
    {
        while (_curState != null && _isInitialized)
        {
            try
            {
                _elapsedTime += Time.deltaTime;
                _curState?.OnUpdate(_elapsedTime);
                
                if (_agent != null && _agent.remainingDistance > 0.1f && _agent.desiredVelocity != Vector3.zero)
                {
                    var targetRotation = Quaternion.LookRotation(_agent.desiredVelocity);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _elapsedTime * turnSpeed);
                }
            }
            catch (Exception)
            {
                Logger.LogError($"{GetType()}:: Error in StartUpdateLoop");
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void StopUpdateLoop()
    {
        if (_updateRoutine != null)
        {
            StopCoroutine(_updateRoutine);
            _updateRoutine = null;
        }
    }

    private void DestroyCurrentState()
    {
        if (_curState is Component comp && comp != null)
        {
            try
            {
                Destroy(comp);
                _curState = null;
            }
            catch (Exception)
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
        catch (Exception)
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
            _targetWayPoint = wayPointList[_wayPointIdx];
            _wayPointIdx = (_wayPointIdx + 1) % wayPointList.Count;
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Next WayPoint is {_targetWayPoint.name}");
            }
            
            return _targetWayPoint;
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in FindWayPoint");
            return null;
        }
    }

    public bool SetDestination()
    {
        var nextWayPoint = FindWayPoint();
        if (nextWayPoint == null || _agent == null)
        {
            return false;
        }

        try
        {
            _agent.SetDestination(nextWayPoint.position);
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Destination is set to {nextWayPoint.name}");
            }
            
            return true;
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in SetDestination");
            return false;
        }
    }

    public void Idle()
    {
        try
        {
            if (_animator != null)
            {
                _animator.SetFloat(MoveSpeed, 0.0f);
            }

            if (_agent != null)
            {
                _agent.isStopped = true;
            }

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Idle");
            }
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in Idle");
        }
    }

    public void Move()
    {
        try
        {
            if (_agent != null)
            {
                _agent.isStopped = false;
            }

            if (_animator != null)
            {
                _animator.SetFloat(MoveSpeed, 0.4f);
            }
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Move");
            }
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in Move");
        }
    }

    public bool HasArrived()
    {
        if (_agent == null)
        {
            return true;
        }

        try
        {
            if (_agent.pathPending)
            {
                return false;
            }

            if (_agent.remainingDistance > _agent.stoppingDistance)
            {
                return false;
            }
            
            bool arrived = !_agent.hasPath || _agent.velocity.sqrMagnitude < 0.1f;
            
            if (arrived)
            {
                Logger.Log($"{GetType()}:: Arrived");
                OnDestinationReached?.Invoke(transform.position);
            }
            
            return arrived;
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in HasArrived");
            return true;
        }
    }

    public bool HasFinishedIdle()
    {
        bool finished = _elapsedTime > _idleTime;
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
            if (_agent != null)
            {
                _agent.ResetPath();
            }
            
            if (debugMode)
            {
                Logger.Log($"{GetType()}:: Path is reset");
            }
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in ResetPath");
        }
    }

    public void SetIdleTime()
    {
        try
        {
            _idleTime = Random.Range(minIdleTime, maxIdleTime);

            if (debugMode)
            {
                Logger.Log($"{GetType()}:: IdleTime is set to {_idleTime}");
            }
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in SetIdleTime");
            _idleTime = 2.0f;
        }
    }

    public void PauseMovement()
    {
        try
        {
            if (_agent != null)
            {
                _agent.isStopped = true;
            }
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in PauseMovement");
        }
    }

    public void ResumeMovement()
    {
        try
        {
            if (_agent != null && _curState is PatrolMove)
            {
                _agent.isStopped = false;
            }
        }
        catch (Exception)
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

        if (_targetWayPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetWayPoint.position, 0.7f);
        }

        if (_agent != null && _agent.hasPath)
        {
            Gizmos.color = Color.green;
            var path = _agent.path.corners;
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
#endif
}
