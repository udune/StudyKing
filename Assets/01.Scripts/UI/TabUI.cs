using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

public enum TabUIEnum
{
    Item = 0,
    History,
    Study,
    Dashboard,
    MySetting,
}

public class TabUI : MonoBehaviour
{
    public List<Toggle> toggles = new List<Toggle>();

    private void Awake()
    {
        for (int idx = 0; idx < toggles.Count; idx++)
        {
            var i = idx;
            toggles[idx].onValueChanged.AddListener((isOn) =>
            {
                Logger.Log($"{GetType()}::Toggle: {i}, isOn: {isOn}");
                OnClickTab(isOn, i);
            });
        }
    }

    private void OnClickTab(bool isOn, int i)
    {
        if (isOn)
        {
            UIManager.Instance.CloseAllOpenUI();

            TabUIEnum tab = (TabUIEnum) i;
            switch (tab)
            {
                case TabUIEnum.Item:
                    OnClickItemTabBtn();
                    break;
                case TabUIEnum.History:
                    OnClickHistoryTabBtn();
                    break;
                case TabUIEnum.Study:
                    OnClickStudyTabBtn();
                    break;
                case TabUIEnum.Dashboard:
                    OnClickDashboardTabBtn();
                    break;
                case TabUIEnum.MySetting:
                    OnClickMySettingTabBtn();
                    break;
            }
        }
    }

    private void OnClickItemTabBtn()
    {
        Logger.Log($"{GetType()}::OnClickItemTabBtn");
        
        var modal = new ModalUIData();
        modal.Type = ModalType.OK;
        modal.Title = string.Empty;
        modal.Desc = "업데이트 준비중";
        modal.OkBtnText = "확인";
        UIManager.Instance.OpenUI<ModalUI>(modal);
        
        // var data = new BaseUIData();
        // UIManager.Instance.OpenUI<ItemTabUI>(data);
    }
    
    private void OnClickHistoryTabBtn()
    {
        Logger.Log($"{GetType()}::OnClickHistoryTabBtn");
        
        var data = new BaseUIData();
        UIManager.Instance.OpenUI<HistoryTabUI>(data);
    }
    
    private void OnClickStudyTabBtn()
    {
        Logger.Log($"{GetType()}::OnClickStudyTabBtn");
    }
    
    private void OnClickDashboardTabBtn()
    {
        Logger.Log($"{GetType()}::OnClickDashboardTabBtn");
        
        var data = new BaseUIData();
        UIManager.Instance.OpenUI<DashboardTabUI>(data);
    }
    
    private void OnClickMySettingTabBtn()
    {
        Logger.Log($"{GetType()}::OnClickMySettingTabBtn");
        
        var data = new BaseUIData();
        UIManager.Instance.OpenUI<MySettingTabUI>(data);
    }
}
