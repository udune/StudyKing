using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolMove : MonoBehaviour, IPatrol
{
    public PlayerController controller;
    private bool isMove;
    
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void OnStart()
    {
        controller.FindWayPoint();
        controller.SetDestination();
        controller.Move();
        
        isMove = true;
        Invoke(nameof(OnMove), 0.5f);
    }

    public void OnEnd()
    {
        controller.Idle();
    }

    public void OnUpdate(float deltaTime)
    {
        if (controller.HasArrived())
        {
            controller.SetDestination();
            if (isMove)
                return;
            controller.ChangeState<PatrolIdle>();
        }
    }

    private void OnMove()
    {
        isMove = false;
    }
}
