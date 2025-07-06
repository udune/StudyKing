using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float turnSpeed = 10.0f;
    
    [Range(0.0f, 10.0f)]
    [SerializeField] private float minIdleTime = 0.0f;
    
    [Range(0.0f, 10.0f)]
    [SerializeField] private float maxIdleTime = 10.0f;

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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        patrolPointList = GameObject.Find("PatrolPoint")?.GetComponentsInChildren<Transform>().ToList();
        patrolPointList?.RemoveAt(0);
    }

    private void Start()
    {
        GenerateWaypoints();
        SetInitialPosition();
        ChangeState<PatrolIdle>();
    }
    
    private void GenerateWaypoints()
    {
        wayPointIdxList.Clear();
        while (wayPointIdxList.Count < patrolPointList.Count)
        {
            int idx = Random.Range(0, patrolPointList.Count);
            if (!wayPointIdxList.Contains(idx))
            {
                wayPointIdxList.Add(idx);
            }
        }

        wayPointList.Clear();
        foreach (int idx in wayPointIdxList)
        {
            wayPointList.Add(patrolPointList[idx]);
        }
    }
    
    private void SetInitialPosition()
    {
        if (wayPointList.Count > 0)
        {
            transform.position = wayPointList[0].position;
        }
    }

    public T ChangeState<T>() where T : Component, IPatrol
    {
        if (curState as T)
            return (T)curState;

        curState?.OnEnd();
        StopUpdateLoop();
        DestroyState();

        curState = AddState<T>();
        curState.OnStart();
        elapsedTime = 0f;

        updateRoutine = StartCoroutine(StartUpdateLoop());

        return curState as T;
    }

    private IEnumerator StartUpdateLoop()
    {
        while (true)
        {
            elapsedTime += Time.deltaTime;
            curState?.OnUpdate(elapsedTime);
            
            if (agent.remainingDistance > 0.1f)
            {
                if (agent.desiredVelocity != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(agent.desiredVelocity), elapsedTime * turnSpeed);
                }
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

    private void DestroyState()
    {
        if (curState is Component comp && comp != null)
        {
            curState?.OnEnd();
            Destroy(comp);
            curState = null;
        }
    }

    private T AddState<T>() where T : Component
    {
        return gameObject.AddComponent<T>();
    }

    public Transform FindWayPoint()
    {
        if (wayPointList.Count.Equals(0))
            return null;
        
        targetWayPoint = wayPointList[wayPointIdx];
        wayPointIdx = (wayPointIdx + 1) % wayPointList.Count;
        return targetWayPoint;
    }

    public void SetDestination()
    {
        Transform next = FindWayPoint();
        if (next == null)
            return;
        
        agent.SetDestination(next.position);
    }

    public void Idle()
    {
        animator.SetFloat("MoveSpeed", 0.0f);
        agent.isStopped = true;
    }

    public void Move()
    {
        agent.isStopped = false;
        animator.SetFloat("MoveSpeed", 0.4f);
    }

    public bool HasArrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;

        return true;
    }

    public bool HasFinishedIdle()
    {
        return elapsedTime > idleTime;
    }

    public void ResetPath()
    {
        agent.ResetPath();
    }

    public void SetIdleTime()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
    }

    private void OnDestroy()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
        
        DestroyState();
    }
}
