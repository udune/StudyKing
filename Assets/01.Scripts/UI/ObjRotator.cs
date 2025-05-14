using System;
using UnityEngine;

public class ObjRotator : MonoBehaviour
{
    private Vector2 startPos;
    public float speed = 0.2f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 dir = (Vector2) Input.mousePosition - startPos;
            transform.Rotate(Vector3.up, -dir.x * speed, Space.World);
            startPos = Input.mousePosition;
        }
    }
}
