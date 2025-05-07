using System;
using UnityEngine;
using Logger = Common.Logger;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rect;
    
    private void Start()
    {
        rect = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        var safeArea = Screen.safeArea;
        Vector2 bottomLeft = safeArea.position;
        Logger.Log($"bottom left is x:{bottomLeft.x}, y:{bottomLeft.y}");

        Vector2 topRight = bottomLeft + safeArea.size;
        Logger.Log($"top right is x:{topRight.x}, y:{topRight.y}");
        
        Logger.Log($"Screen.width:{Screen.width}, Screen.height:{Screen.height}");

        Vector2 anchorMin = Vector2.zero;
        anchorMin.x = bottomLeft.x / Screen.width;
        anchorMin.y = bottomLeft.y / Screen.height;
        
        Vector2 anchorMax = Vector2.zero;
        anchorMax.x = topRight.x / Screen.width;
        anchorMax.y = topRight.y / Screen.height;
        
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
    }
}
