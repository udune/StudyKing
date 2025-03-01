using System;
using UnityEngine;
using Logger = Common.Logger;

public class BaseUIData
{
    public Action OnShow;
    public Action OnClose;
}

public class BaseUI : MonoBehaviour
{
    public Animation openAnimation;

    private Action onShow;
    private Action onClose;

    public virtual void Init(Transform anchor)
    {
        Logger.Log($"{GetType()}::Init");

        onShow = null;
        onClose = null;

        transform.SetParent(anchor);
        
        var rect = GetComponent<RectTransform>();
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public virtual void Setting(BaseUIData data)
    {
        Logger.Log($"{GetType()}::Setting");
        
        onShow = data.OnShow;
        onClose = data.OnClose;
    }

    public virtual void ShowUI()
    {
        if (openAnimation != null)
            openAnimation.Play();
        
        onShow?.Invoke();
        onShow = null;
    }

    public virtual void CloseUI(bool isClose = false)
    {
        if (!isClose)
            onClose?.Invoke();
        
        onClose = null;
    }

    public virtual void OnClickClose()
    {
        CloseUI();
    }
}
