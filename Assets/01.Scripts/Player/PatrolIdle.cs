using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolIdle : MonoBehaviour, IPatrol
{
    PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void OnStart()
    {
        controller.Idle();
        controller.SetIdleTime();
    }

    public void OnEnd()
    {
        
    }

    public void OnUpdate(float deltaTime)
    {
        if (controller.HasFinishedIdle())
        {
            controller.ChangeState<PatrolMove>();
        }
    }
}
