using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UIManager : SingletonBehaviour<UIManager>
{
    [Header("UI 컨테이너")]
    // 활성화된 UI들이 붙을 캔버스
    [SerializeField] Transform UICanvasTrn;
    // 비활성화된 UI들이 보관될 컨테이너
    [SerializeField] Transform ClosedUITrn;

    [Header("고정 UI들")]
    // 시간 UI
    [SerializeField] private TimeUI timeUI;
    // 하단 탭 고정 UI
    [SerializeField] private TabUI tabUI;
    
    // 현재 가장 위에 UI
    private BaseUI currentUI;
    
    // UI 풀링 위한 딕셔너리
    private Dictionary<Type, GameObject> openUIPool = new Dictionary<Type, GameObject>();
    private Dictionary<Type, GameObject> closedUIPool = new Dictionary<Type, GameObject>();
    
    [Obsolete("Obsolete")]
    protected override void Init()
    {
        base.Init();

        // 컴포넌트 연결
        InitComponents();
        
        Logger.Log($"{GetType()}::UIManager 초기화 완료");
    }

    [Obsolete("Obsolete")]
    private void InitComponents()
    {
        // UI 컨테이너들이 설정되지 않았다면 찾는다.
        if (UICanvasTrn == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                UICanvasTrn = canvas.transform;
                Logger.Log($"{GetType()}::UI 캔버스를 찾았다.");
            }
            else
            {
                Logger.LogError($"{GetType()}::UI 캔버스를 찾을 수 없다.");
            }
        }

        // timeUI를 찾는다.
        if (timeUI == null)
        {
            timeUI = FindObjectOfType<TimeUI>();
            if (timeUI == null)
            {
                Logger.LogWarning($"{GetType()}::TimeUI를 찾을 수 없다.");
            }
            else
            {
                Logger.Log($"{GetType()}::TimeUI를 찾았다.");
            }
        }

        // TabUI를 찾는다.
        if (tabUI == null)
        {
            tabUI = FindObjectOfType<TabUI>();
            if (tabUI == null)
            {
                Logger.LogWarning($"{GetType()}::TabUI를 찾을 수 없다.");
            }
            else
            {
                Logger.Log($"{GetType()}::TabUI를 찾았다.");
            }
        }

        // 닫힌 UI 컨테이너가 없다면 만들어준다.
        if (ClosedUITrn == null)
        {
            GameObject closedUIContainer = new GameObject("ClosedUIContainer");
            closedUIContainer.transform.SetParent(transform);
            closedUIContainer.SetActive(false);
            ClosedUITrn = closedUIContainer.transform;
            Logger.Log($"{GetType()}::닫힌 UI 컨테이너를 자동으로 생성했습니다.");
        }
    }

    public void SetTimeUIVisible(bool isVisible)
    {
        if (timeUI == null)
        {
            Logger.LogWarning($"{GetType()}::TimeUI가 없습니다.");
            return;
        }
        
        timeUI.gameObject.SetActive(isVisible);

        // 보여지면 값 새로고침
        if (isVisible)
        {
            // timeUI.RefreshTimeDisplay();
        }
        
        Logger.Log($"{GetType()}::TimeUI 표시 : {isVisible}");
    }

    public void SetTabUIVisible(bool isVisible)
    {
        if (tabUI == null)
        {
            Logger.LogWarning($"{GetType()}::TabUI가 없습니다.");
            return;
        }
        
        tabUI.gameObject.SetActive(isVisible);
        Logger.Log($"{GetType()}::TabUI 표시 : {isVisible}");
    }

    private BaseUI GetUIFromPool<T>(out bool isAlreadyOpen) where T : BaseUI
    {
        Type type = typeof(T);
        BaseUI ui = null;
        isAlreadyOpen = false;

        // 열려있는 UI에서 확인
        if (openUIPool.ContainsKey(type))
        {
            ui = openUIPool[type].GetComponent<BaseUI>();
            isAlreadyOpen = true;
            Logger.Log($"{GetType()}::이미 열려있는 {type} UI를 반환합니다");
        } // 닫혀있는 UI에서 확인
        else if (closedUIPool.ContainsKey(type))
        {
            ui = closedUIPool[type].GetComponent<BaseUI>();
            closedUIPool.Remove(type);
            Logger.Log($"{GetType()}::풀에서 {type} UI를 가져왔습니다");
        }
        else // 새로 생성
        {
            GameObject pref = Resources.Load<GameObject>($"UI/{type}");
            if (pref == null)
            {
                Logger.LogError($"{GetType()}::UI 프리팹을 찾을 수 없습니다: UI/{type}");
                return null;
            }
            
            GameObject go = Instantiate(pref);
            ui = go.GetComponent<BaseUI>();

            if (ui == null)
            {
                Logger.LogError($"{GetType()}::{type} 프리팹에 BaseUI 컴포넌트가 없습니다.");
                Destroy(go);
                return null;
            }
            
            Logger.Log($"{GetType()}::새로운 {type} UI를 생성했습니다");
        }

        return ui;
    }
    
    public void OpenUI<T>(BaseUIData data = null) where T : BaseUI
    {
        Type type = typeof(T);
        
        if (data == null)
        {
            data = new BaseUIData();
            Logger.Log($"{GetType()}::BaseUIData가 null이어서 기본 데이터를 생성했습니다");
        }
        
        Logger.Log($"{GetType()}::OpenUI({type})");
        
        bool isAlreadyOpen;
        var ui = GetUIFromPool<T>(out isAlreadyOpen);

        if (ui == null)
        {
            Logger.Log($"{type} does not exist");
            return;
        }

        if (isAlreadyOpen)
        {
            Logger.Log($"{type} is already open");
            // 맨 앞으로 보낸다.
            ui.transform.SetAsLastSibling();
            currentUI = ui;
            return;
        }

        // UI를 실제 표시한다.
        SetupAndShow(ui, data, type);
    }

    private void SetupAndShow(BaseUI ui, BaseUIData data, Type type)
    {
        try
        {
            // 초기화
            ui.Init(UICanvasTrn);
            // 맨 앞으로
            ui.transform.SetAsLastSibling();
            // 활성화
            ui.gameObject.SetActive(true);
            // 설정
            ui.Setting(data);
            // 표시
            ui.ShowUI();

            currentUI = ui;

            openUIPool[type] = ui.gameObject;

            Logger.Log($"{GetType()}::{type} UI 열기 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::{type} UI 열기 중 오류 발생: {e.Message}");

            // 오류 발생 시 만들어진 UI는 삭제한다.
            if (ui != null && ui.gameObject != null)
            {
                Destroy(ui.gameObject);
            }
        }
    }

    public void CloseUI(BaseUI ui)
    {
        if (ui == null)
        {
            Logger.LogWarning($"{GetType()}::닫으려는 UI가 null입니다.");
            return;
        }
        
        Type type = ui.GetType();
        Logger.Log($"{GetType()}::CloseUI({type})");

        try
        {
            // 비활성화
            ui.gameObject.SetActive(false);
            // 열린 UI풀에서 제거
            openUIPool.Remove(type);
            // 닫힌 UI풀에 추가
            closedUIPool[type] = ui.gameObject;
            // 닫힌 UI 컨테이너로 이동
            ui.transform.SetParent(ClosedUITrn);
            // UI 업데이트
            UpdateCurrentUI();
            
            Logger.Log($"{GetType()}::{type} UI 닫기 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::{type} UI 닫기 중 오류 발생: {e.Message}");
        }
    }

    private void UpdateCurrentUI()
    {
        currentUI = null;

        if (UICanvasTrn.childCount > 0)
        {
            // 마지막이 현재 UI
            Transform lastChild = UICanvasTrn.GetChild(UICanvasTrn.childCount - 1);
            if (lastChild != null)
            {
                currentUI = lastChild.GetComponent<BaseUI>();
            }
        }

        if (currentUI != null)
        {
            Logger.Log($"{GetType()}::현재 UI가 {currentUI.GetType()}로 변경되었습니다");
        }
        else
        {
            Logger.Log($"{GetType()}::현재 활성화된 UI가 없습니다");
        }
    }

    public BaseUI GetActiveUI<T>() where T : BaseUI
    {
        Type type = typeof(T);
        return openUIPool.ContainsKey(type) ? openUIPool[type].GetComponent<T>() : null;
    }

    // 모든 열린 UI 닫기
    public void CloseAllOpenUI()
    {
        Logger.Log($"{GetType()}::CloseAllOpenUI()");
        
        var openUIs = new List<BaseUI>();
        foreach (var go in openUIPool.Values)
        {
            if (go != null)
            {
                BaseUI ui = go.GetComponent<BaseUI>();
                if (ui != null)
                {
                    openUIs.Add(ui);
                }
            }
        }

        foreach (var ui in openUIs)
        {
            ui.CloseUI(true);
        }
        
        Logger.Log($"{GetType()}::모든 UI 닫기 완료 - 총 {openUIs.Count}개");
    }

    // 특정 타입 UI를 닫는다.
    public void CloseUI<T>() where T : BaseUI
    {
        BaseUI ui = GetActiveUI<T>();
        if (ui != null)
        {
            ui.CloseUI(true);
        }
        else
        {
            Logger.Log($"{GetType()}::{typeof(T)} UI가 열려있지 않습니다");
        }
    }

    // UI 풀을 정리한다.
    public void ClearUIPool()
    {
        Logger.Log($"{GetType()}::ClearUIPool()");

        foreach (var go in openUIPool.Values)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
        
        closedUIPool.Clear();
        Logger.Log($"{GetType()}::UI 풀 정리 완료");
    }

    protected override void OnDestroy()
    {
        CloseAllOpenUI();
        ClearUIPool();
        
        base.OnDestroy();
        Logger.Log($"{GetType()}::UI 매니저가 정리되었습니다");
    }
}
