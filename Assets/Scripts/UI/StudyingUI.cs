using System;
using Gpm.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudyingUI : BaseUI
{
    public InfiniteScroll studyingScrollList;
    public TextMeshProUGUI time;
    public Button completeBtn;
    private float elapsedTime;

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            if (!LobbyManager.Instance.IsPaused)
            {
                LobbyManager.Instance.Pause();
                
                var data = new BaseUIData();
                UIManager.Instance.OpenUI<PauseUI>(data);
            }
        }
    }

    private void Update()
    {
        if (!LobbyManager.Instance.IsPaused)
        {
            elapsedTime += Time.deltaTime;
            CalcTime();
        }

        if (LobbyManager.Instance.IsComplete)
        {
            LobbyManager.Instance.Pause();
            completeBtn.interactable = true;
        }
    }

    private void CalcTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        time.text = $"{minutes:00}:{seconds:00}";
    }

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);

        elapsedTime = 0.0f;
        LobbyManager.Instance.IsPaused = false;
        LobbyManager.Instance.IsComplete = false;
        
        studyingScrollList.Clear();

        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            foreach (var itemData in userStudyData.StudyItemDataList)
            {
                var itemSlotData = new StudyingItemSlotData();
                itemSlotData.Id = itemData.Id;
                itemSlotData.Name = itemData.Name;
                itemSlotData.Check = itemData.Check = false;
                studyingScrollList.InsertData(itemSlotData);
            }

            userStudyData.SaveData();
        }
    }

    public void OnClickPause()
    {
        LobbyManager.Instance.Pause();
        
        var data = new BaseUIData();
        UIManager.Instance.OpenUI<PauseUI>(data);
    }

    public void OnClickFinishStudyItem()
    {
        
    }
}
