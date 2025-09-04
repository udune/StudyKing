using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

/// <summary>
/// UI 관리자 - UIPool을 활용하고 CloseAllOpenUI 함수 포함
/// </summary>
public class UIManager : SingletonBehaviour<UIManager>
{
    [Header("UI 컨테이너")]
    [SerializeField] private Transform uiCanvasTrn;

    [Header("고정 UI들")]
    [SerializeField] private TimeUI timeUI;
    [SerializeField] private TabUI tabUI;
    
    private BaseUI _currentUI;
    
    // 현재 열려있는 UI들을 추적 (UIPool과 함께 사용)
    private List<BaseUI> _activeUIs = new List<BaseUI>();
    
    protected override void Init()
    {
        base.Init();
        InitComponents();
        Logger.Log($"{GetType()}::UIManager 초기화 완료");
    }

    /// <summary>
    /// 컴포넌트 초기화 - 자동 검색 및 연결
    /// </summary>
    private void InitComponents()
    {
        if (uiCanvasTrn == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            uiCanvasTrn = canvas?.transform;
        }

        if (timeUI == null)
            timeUI = FindAnyObjectByType<TimeUI>();

        if (tabUI == null)
            tabUI = FindAnyObjectByType<TabUI>();
    }

    /// <summary>
    /// UI 열기 - UIPool 활용
    /// </summary>
    public T OpenUI<T>(BaseUIData data = null) where T : BaseUI
    {
        try
        {
            // UIPool에서 UI 가져오기
            T ui = UIPool.Instance?.GetFromPool<T>();
            
            if (ui == null)
            {
                Logger.LogError($"{GetType()}::{typeof(T)} UI를 생성할 수 없습니다");
                return null;
            }

            // UI 설정
            SetupUI(ui, data);
            
            // 활성 UI 목록에 추가
            _activeUIs.Add(ui);
            
            Logger.Log($"{GetType()}::{typeof(T)} UI 열기 완료");
            return ui;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UI 열기 중 오류 발생: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// UI 설정 및 표시
    /// </summary>
    private void SetupUI<T>(T ui, BaseUIData data) where T : BaseUI
    {
        // 캔버스에 배치
        ui.transform.SetParent(uiCanvasTrn, false);
        ui.transform.SetAsLastSibling();
        
        // UI 설정 및 표시
        ui.Setting(data);
        ui.ShowUI();
        
        // 현재 UI 업데이트
        _currentUI = ui;
    }

    /// <summary>
    /// UI 닫기 (특정 타입)
    /// </summary>
    public void CloseUI<T>() where T : BaseUI
    {
        Logger.Log($"{GetType()}::{typeof(T)} UI 닫기 요청");
        
        // 해당 타입의 UI 찾아서 닫기
        for (int i = _activeUIs.Count - 1; i >= 0; i--)
        {
            if (_activeUIs[i] is T)
            {
                _activeUIs[i].CloseUI();
                break;
            }
        }
    }

    /// <summary>
    /// UI 닫기 처리 (BaseUI에서 호출)
    /// </summary>
    public void OnUIClosed(BaseUI ui)
    {
        if (ui == _currentUI)
        {
            _currentUI = null;
        }
        
        // 활성 UI 목록에서 제거
        _activeUIs.Remove(ui);
        
        // UIPool에 반환
        UIPool.Instance?.ReturnToPool(ui);
    }

    /// <summary>
    /// 모든 열린 UI 닫기 - 이 함수가 필요합니다!
    /// </summary>
    public void CloseAllOpenUI()
    {
        Logger.Log($"{GetType()}::모든 UI 닫기 - 총 {_activeUIs.Count}개");
        
        // 역순으로 모든 UI 닫기 (최근에 연 UI부터)
        for (int i = _activeUIs.Count - 1; i >= 0; i--)
        {
            if (_activeUIs[i] != null)
            {
                _activeUIs[i].CloseUI();
            }
        }
        
        // 리스트 정리 (OnUICllosed에서 하나씩 제거되지만 안전을 위해)
        _activeUIs.Clear();
        
        Logger.Log($"{GetType()}::모든 UI 닫기 완료");
    }

    /// <summary>
    /// 시간 UI 표시/숨김
    /// </summary>
    public void SetTimeUIVisible(bool isVisible)
    {
        if (timeUI != null)
        {
            timeUI.gameObject.SetActive(isVisible);
        }
    }

    /// <summary>
    /// 탭 UI 표시/숨김
    /// </summary>
    public void SetTabUIVisible(bool isVisible)
    {
        if (tabUI != null)
        {
            tabUI.gameObject.SetActive(isVisible);
        }
    }

    /// <summary>
    /// 현재 활성 UI 가져오기
    /// </summary>
    public T GetActiveUI<T>() where T : BaseUI
    {
        foreach (var ui in _activeUIs)
        {
            if (ui is T)
            {
                return ui as T;
            }
        }
        return null;
    }

    /// <summary>
    /// 현재 열린 UI 개수
    /// </summary>
    public int GetActiveUICount()
    {
        return _activeUIs.Count;
    }

    /// <summary>
    /// 특정 타입의 UI가 열려있는지 확인
    /// </summary>
    public bool IsUIOpen<T>() where T : BaseUI
    {
        return GetActiveUI<T>() != null;
    }

    protected override void OnDestroy()
    {
        CloseAllOpenUI();
        base.OnDestroy();
        
        Logger.Log($"{GetType()}::UIManager 정리 완료");
    }
}