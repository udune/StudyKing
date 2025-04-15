using System;
using System.Collections;
using Gpm.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

public class StudyingUI : BaseUI
{
    public InfiniteScroll studyingScrollList;
    public TextMeshProUGUI time;
    public Button completeBtn;
    private float elapsedTime;
    private Coroutine timerCoroutine;

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
        
        TimerStart();

        LobbyManager.Instance.OnCompleteChanged -= UpdateCompleteButton;
        LobbyManager.Instance.OnCompleteChanged += UpdateCompleteButton;

        UpdateCompleteButton();
    }

    public void TimerStart()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (!LobbyManager.Instance.IsPaused)
        {
            elapsedTime += Time.deltaTime;
            int hours = Mathf.FloorToInt(elapsedTime / 3600);
            int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);

            time.text = hours > 0 ? $"{hours:00}:{minutes:00}:{seconds:00}" : $"{minutes:00}:{seconds:00}";

            yield return null;
        }
    }

    private void UpdateCompleteButton()
    {
        completeBtn.interactable = LobbyManager.Instance.IsComplete;
    }

    public bool CheckCompleted()
    {
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            return userStudyData.StudyItemDataList.TrueForAll(item => item.Check);
        }

        return false;
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnCompleteChanged -= UpdateCompleteButton;
    }

    public void OnClickPause()
    {
        LobbyManager.Instance.Pause();
        
        var data = new BaseUIData();
        UIManager.Instance.OpenUI<PauseUI>(data);
    }

    public void OnClickFinishStudyItem()
    {
        var userTimeData = UserDataManager.Instance.GetUserData<UserTimeData>();
        if (userTimeData == null)
        {
            Logger.Log($"{GetType()}::UserData is null");
            return;
        }

        userTimeData.Time += (long) elapsedTime;
        userTimeData.SaveData();
        UIManager.Instance.EnableTimeUI(true);
        
        UIManager.Instance.CloseAllOpenUI();
    }
}
