using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

/// <summary>
/// 탭의 종류를 나타내는 열거형
/// 새로운 탭을 추가할 때는 여기에 추가하면 됩니다
/// </summary>
public enum TabUIEnum
{
    Item = 0,       // 아이템 탭
    History,        // 히스토리 탭
    Study,          // 공부 탭
    Dashboard,      // 대시보드 탭
    MySetting,      // 내 설정 탭
}

/// <summary>
/// 하단 탭 UI를 관리하는 클래스
/// 각 탭을 클릭했을 때 해당하는 UI를 열어줍니다
/// </summary>
public class TabUI : MonoBehaviour
{
    [Header("탭 토글 버튼들")]
    [SerializeField] private List<Toggle> toggles = new List<Toggle>(); // 각 탭의 토글 버튼들
    
    /// <summary>
    /// 게임이 시작될 때 실행되는 함수
    /// 각 토글 버튼에 클릭 이벤트를 연결합니다
    /// </summary>
    private void Awake()
    {
        // 모든 토글 버튼에 이벤트를 연결합니다
        for (int idx = 0; idx < toggles.Count; idx++)
        {
            int tabIndex = idx; // 지역변수로 복사 (클로저 문제 해결)
            
            // 토글이 눌렸을 때 실행될 함수를 연결합니다
            toggles[idx].onValueChanged.AddListener((isOn) =>
            {
                Logger.Log($"{GetType()}::탭 {tabIndex}번이 {(isOn ? "선택" : "해제")}되었습니다");
                OnTabClicked(isOn, tabIndex);
            });
        }
    }

    /// <summary>
    /// 탭이 클릭되었을 때 실행되는 함수
    /// </summary>
    /// <param name="isOn">탭이 선택되었는지 여부</param>
    /// <param name="tabIndex">클릭된 탭의 인덱스</param>
    private void OnTabClicked(bool isOn, int tabIndex)
    {
        // 탭이 선택된 경우에만 처리합니다
        if (!isOn) return;
        
        // 현재 열려있는 모든 UI를 닫습니다
        UIManager.Instance.CloseAllOpenUI();

        // 클릭된 탭에 따라 해당 UI를 엽니다
        TabUIEnum selectedTab = (TabUIEnum)tabIndex;
        
        switch (selectedTab)
        {
            case TabUIEnum.Item:
                OpenItemTab();
                break;
            case TabUIEnum.History:
                OpenHistoryTab();
                break;
            case TabUIEnum.Study:
                OpenStudyTab();
                break;
            case TabUIEnum.Dashboard:
                OpenDashboardTab();
                break;
            case TabUIEnum.MySetting:
                OpenMySettingTab();
                break;
            default:
                Logger.LogWarning($"{GetType()}::알 수 없는 탭입니다: {selectedTab}");
                break;
        }
    }

    /// <summary>
    /// 아이템 탭을 여는 함수
    /// 캐릭터 회전 기능을 활성화합니다
    /// </summary>
    private void OpenItemTab()
    {
        Logger.Log($"{GetType()}::아이템 탭을 엽니다");
        
        // UI 데이터를 만듭니다
        var uiData = new BaseUIData
        {
            // UI가 열릴 때 캐릭터 회전을 활성화합니다
            OnShow = () =>
            {
                if (PlayerCustom.Instance?.character != null)
                {
                    var rotator = PlayerCustom.Instance.character.GetComponent<ObjRotator>();
                    if (rotator != null)
                    {
                        rotator.enabled = true;
                        Logger.Log($"{GetType()}::캐릭터 회전 기능을 활성화했습니다");
                    }
                }
            },
            // UI가 닫힐 때 캐릭터 회전을 비활성화합니다
            OnClose = () =>
            {
                if (PlayerCustom.Instance?.character != null)
                {
                    var rotator = PlayerCustom.Instance.character.GetComponent<ObjRotator>();
                    if (rotator != null)
                    {
                        rotator.enabled = false;
                        Logger.Log($"{GetType()}::캐릭터 회전 기능을 비활성화했습니다");
                    }
                }
            }
        };

        // 아이템 탭 UI를 엽니다
        UIManager.Instance.OpenUI<ItemTabUI>(uiData);
    }
    
    /// <summary>
    /// 히스토리 탭을 여는 함수
    /// 학습 기록을 보여줍니다
    /// </summary>
    private void OpenHistoryTab()
    {
        Logger.Log($"{GetType()}::히스토리 탭을 엽니다");
        
        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<HistoryTabUI>(uiData);
    }
    
    /// <summary>
    /// 공부 탭을 여는 함수
    /// 현재는 구현되지 않았습니다
    /// </summary>
    private void OpenStudyTab()
    {
        Logger.Log($"{GetType()}::공부 탭을 엽니다 (아직 구현되지 않음)");
        
        // TODO: 공부 탭 UI 구현하기
        // var uiData = new BaseUIData();
        // UIManager.Instance.OpenUI<StudyTabUI>(uiData);
    }
    
    /// <summary>
    /// 대시보드 탭을 여는 함수
    /// 학습 통계와 AI 조언을 보여줍니다
    /// </summary>
    private void OpenDashboardTab()
    {
        Logger.Log($"{GetType()}::대시보드 탭을 엽니다");
        
        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<DashboardTabUI>(uiData);
    }
    
    /// <summary>
    /// 내 설정 탭을 여는 함수
    /// 사용자 설정을 변경할 수 있습니다
    /// </summary>
    private void OpenMySettingTab()
    {
        Logger.Log($"{GetType()}::내 설정 탭을 엽니다");
        
        var uiData = new BaseUIData();
        UIManager.Instance.OpenUI<MySettingTabUI>(uiData);
    }

    /// <summary>
    /// 특정 탭을 프로그래밍적으로 선택하는 함수
    /// 다른 스크립트에서 탭을 강제로 선택하고 싶을 때 사용합니다
    /// </summary>
    /// <param name="tabType">선택하고 싶은 탭</param>
    public void SelectTab(TabUIEnum tabType)
    {
        int tabIndex = (int)tabType;
        
        // 유효한 인덱스인지 확인합니다
        if (tabIndex < 0 || tabIndex >= toggles.Count)
        {
            Logger.LogError($"{GetType()}::잘못된 탭 인덱스입니다: {tabIndex}");
            return;
        }
        
        // 해당 토글을 활성화합니다
        toggles[tabIndex].isOn = true;
        Logger.Log($"{GetType()}::{tabType} 탭을 프로그래밍적으로 선택했습니다");
    }

    /// <summary>
    /// 현재 선택된 탭을 반환하는 함수
    /// </summary>
    /// <returns>현재 선택된 탭, 선택된 탭이 없으면 null</returns>
    public TabUIEnum? GetCurrentSelectedTab()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
            {
                return (TabUIEnum)i;
            }
        }
        
        Logger.LogWarning($"{GetType()}::현재 선택된 탭이 없습니다");
        return null;
    }

    /// <summary>
    /// 모든 탭을 비활성화하는 함수
    /// </summary>
    public void DeselectAllTabs()
    {
        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }
        
        Logger.Log($"{GetType()}::모든 탭을 비활성화했습니다");
    }

    /// <summary>
    /// 특정 탭을 활성화/비활성화하는 함수
    /// </summary>
    /// <param name="tabType">대상 탭</param>
    /// <param name="isEnabled">활성화 여부</param>
    public void SetTabEnabled(TabUIEnum tabType, bool isEnabled)
    {
        int tabIndex = (int)tabType;
        
        // 유효한 인덱스인지 확인합니다
        if (tabIndex < 0 || tabIndex >= toggles.Count)
        {
            Logger.LogError($"{GetType()}::잘못된 탭 인덱스입니다: {tabIndex}");
            return;
        }
        
        // 토글의 상호작용 가능 여부를 설정합니다
        toggles[tabIndex].interactable = isEnabled;
        Logger.Log($"{GetType()}::{tabType} 탭을 {(isEnabled ? "활성화" : "비활성화")}했습니다");
    }

    /// <summary>
    /// 게임 오브젝트가 비활성화될 때 실행되는 함수
    /// 메모리 누수를 방지하기 위해 이벤트 리스너를 제거합니다
    /// </summary>
    private void OnDestroy()
    {
        // 모든 토글의 이벤트 리스너를 제거합니다
        foreach (var toggle in toggles)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }
        
        Logger.Log($"{GetType()}::탭 UI가 정리되었습니다");
    }
}