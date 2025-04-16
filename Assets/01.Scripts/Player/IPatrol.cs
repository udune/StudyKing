using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPatrol
{
    public void OnStart();
    public void OnEnd();
    public void OnUpdate(float deltaTime);
}
