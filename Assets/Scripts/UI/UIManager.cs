using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UIManager : SingletonBehaviour<UIManager>
{
    public Transform UICanvasTrn;
    public Transform ClosedUITrn;

    private BaseUI currentUI;
    private Dictionary<Type, GameObject> openUIPool = new Dictionary<Type, GameObject>();
    private Dictionary<Type, GameObject> closedUIPool = new Dictionary<Type, GameObject>();

    private BaseUI GetUI<T>(out bool isAlreadyOpen)
    {
        Type type = typeof(T);

        BaseUI ui = null;
        isAlreadyOpen = false;

        if (openUIPool.ContainsKey(type))
        {
            ui = openUIPool[type].GetComponent<BaseUI>();
            isAlreadyOpen = true;
        }
        else if (closedUIPool.ContainsKey(type))
        {
            ui = closedUIPool[type].GetComponent<BaseUI>();
            closedUIPool.Remove(type);
        }
        else
        {
            var uiGo = Instantiate(Resources.Load<GameObject>($"UI/{type}"));
            ui = uiGo?.GetComponent<BaseUI>();
        }

        return ui;
    }

    public void OpenUI<T>(BaseUIData data)
    {
        Type type = typeof(T);
        
        Logger.Log($"{GetType()}::OpenUI({type})");
        
        bool isAlreadyOpen = false;
        var ui = GetUI<T>(out isAlreadyOpen);

        if (ui == null)
        {
            Logger.Log($"{type} does not exist");
            return;
        }

        if (isAlreadyOpen)
        {
            Logger.Log($"{type} is already open");
            return;
        }

        var siblingIdx = UICanvasTrn.childCount;
        ui.Init(UICanvasTrn);
        ui.transform.SetSiblingIndex(siblingIdx);
        ui.gameObject.SetActive(true);
        ui.Setting(data);
        ui.ShowUI();

        currentUI = ui;
        openUIPool[type] = ui.gameObject;
    }

    public void CloseUI(BaseUI ui)
    {
        Type type = ui.GetType();
        
        Logger.Log($"{GetType()}::CloseUI({type})");
        
        ui.gameObject.SetActive(false);
        openUIPool.Remove(type);
        closedUIPool[type] = ui.gameObject;
        ui.transform.SetParent(ClosedUITrn);

        currentUI = null;
        var lastChild = UICanvasTrn.GetChild(UICanvasTrn.childCount - 1);
        if (lastChild != null)
        {
            currentUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    public BaseUI GetActiveUI<T>()
    {
        Type type = typeof(T);
        return openUIPool.ContainsKey(type) ? openUIPool[type].GetComponent<BaseUI>() : null;
    }
    
    public bool IsExistOpenUI()
    {
        return currentUI != null;
    }

    public BaseUI GetCurrentUI()
    {
        return currentUI;
    }

    public void CloseAllOpenUI()
    {
        while (currentUI != null)
        {
            currentUI.CloseUI(true);
        }
    }
}
