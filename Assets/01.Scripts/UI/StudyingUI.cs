using System;
using System.Collections;
using System.Collections.Generic;
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
    private Coroutine timerCoroutine;
    
    private float elapsedTime;
    public DateTime startTime;

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

        startTime = DateTime.UtcNow;
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
                itemSlotData.Id = itemData.id;
                itemSlotData.Name = itemData.name;
                itemSlotData.Check = itemData.check = false;
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

    public void ResumeSubjectTimer(DateTime paused)
    {
        TimeSpan elapsedPaused = DateTime.UtcNow - paused;
        startTime = startTime.Add(elapsedPaused);
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
            return userStudyData.StudyItemDataList.TrueForAll(item => item.check);
        }

        return false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnCompleteChanged -= UpdateCompleteButton;
    }
    
    private void OnDisable()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
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
        var userTimeData = UserDataManager.Instance.GetUserData<UserTimeData>();
        var userDailyTimeData = UserDataManager.Instance.GetUserData<UserDailyTimeData>();
        var userHistoryData = UserDataManager.Instance.GetUserData<UserHistoryData>();
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        
        if (userTimeData == null || userDailyTimeData == null || userHistoryData == null || userStudyData == null)
        {
            Logger.Log($"{GetType()}::UserData is null");
            return;
        }

        userTimeData.Time += (long) elapsedTime;
        userTimeData.SaveData();

        DateTime dateNow = DateTime.UtcNow.AddHours(9);
        string today = dateNow.ToString("yyyy-MM-dd");
        
        var todayItem = userHistoryData.HistoryItemDataList.Find(x => x.Date.Equals(today));
        if (todayItem == null)
        {
            todayItem = new HistoryItemData()
            {
                Date = today,
                SubjectList = new List<string>()
            };
            userHistoryData.HistoryItemDataList.Add(todayItem);
        }
        
        foreach (var studyItemData in userStudyData.StudyItemDataList)
        {
            if (!todayItem.SubjectList.Contains(studyItemData.name))
            {
                todayItem.SubjectList.Add(studyItemData.name);
            }
        }
        
        DateTime threeMonthAgo = dateNow.AddMonths(-3);
        userHistoryData.HistoryItemDataList.RemoveAll(item =>
        {
            if (DateTime.TryParse(item.Date, out DateTime date))
            {
                return date < threeMonthAgo;
            }

            return false;
        });
        
        userHistoryData.HistoryItemDataList.Sort((a, b) =>
        {
            DateTime.TryParse(a.Date, out DateTime aDate);
            DateTime.TryParse(b.Date, out DateTime bDate);
            return bDate.CompareTo(aDate);
        });
        
        userHistoryData.SaveData();
        
        var dailyItem = userDailyTimeData.DailyTimeItemDataList.Find(x => x.Date.Equals(today));
        if (dailyItem == null)
        {
            dailyItem = new DailyTimeItemData()
            {
                Date = today,
                Time = 0
            };
            userDailyTimeData.DailyTimeItemDataList.Add(dailyItem);
        }
        
        dailyItem.Time += (long) elapsedTime;
        userDailyTimeData.SaveData();
        
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "time", userTimeData.Time.ToString() }
        };
        FirebaseManager.Instance.LogCustomEvent("study_clear", parameters);
        
        UIManager.Instance.SetTimeUIVisible(true);
        UIManager.Instance.CloseAllOpenUI();
    }
}
