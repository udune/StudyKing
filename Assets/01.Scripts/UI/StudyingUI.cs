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
    private Coroutine _timerCoroutine;
    
    private float _elapsedTime;
    public DateTime StartTime;

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

        StartTime = DateTime.UtcNow;
        _elapsedTime = 0.0f;
        LobbyManager.Instance.IsPaused = false;
        LobbyManager.Instance.IsComplete = false;
        
        studyingScrollList.Clear();

        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            foreach (var itemData in userStudyData.StudyItemDataList)
            {
                var itemSlotData = new StudyingItemSlotData
                {
                    Id = itemData.id,
                    Name = itemData.name,
                    Check = itemData.check = false
                };
                studyingScrollList.InsertData(itemSlotData);
            }

            userStudyData.SaveData();
        }
        
        TimerStart();

        LobbyManager.Instance.OnCompleteChanged -= UpdateCompleteButton;
        LobbyManager.Instance.OnCompleteChanged += UpdateCompleteButton;

        // 완료 상태 즉시 체크 및 업데이트
        CheckAndUpdateCompletionStatus();

        UpdateCompleteButton();
    }

    public void TimerStart()
    {
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (!LobbyManager.Instance.IsPaused)
        {
            _elapsedTime += Time.deltaTime;
            int hours = Mathf.FloorToInt(_elapsedTime / 3600);
            int minutes = Mathf.FloorToInt((_elapsedTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(_elapsedTime % 60);

            time.text = hours > 0 ? $"{hours:00}:{minutes:00}:{seconds:00}" : $"{minutes:00}:{seconds:00}";

            yield return null;
        }
    }

    public void Resume(DateTime paused)
    {
        TimeSpan elapsedPaused = DateTime.UtcNow - paused;
        StartTime = StartTime.Add(elapsedPaused);
    }

    private void UpdateCompleteButton()
    {
        completeBtn.interactable = LobbyManager.Instance.IsComplete;
    }

    /// <summary>
    /// 완료 상태를 체크하고 LobbyManager와 UI를 즉시 업데이트하는 메서드
    /// </summary>
    public void CheckAndUpdateCompletionStatus()
    {
        bool isCompleted = CheckCompleted();
        
        if (LobbyManager.Instance.IsComplete != isCompleted)
        {
            Logger.Log($"{GetType()}::완료 상태 변경: {LobbyManager.Instance.IsComplete} → {isCompleted}");
            LobbyManager.Instance.IsComplete = isCompleted;
            // LobbyManager의 OnCompleteChanged 이벤트가 자동으로 UpdateCompleteButton 호출
            
            // 모든 항목이 완료된 경우 완료 확인 모달 표시
            if (isCompleted)
            {
                ShowCompletionModal();
            }
        }
    }
    
    /// <summary>
    /// 공부 완료 확인 모달을 표시하는 메서드
    /// </summary>
    private void ShowCompletionModal()
    {
        var pauseStartTime = DateTime.UtcNow;
        LobbyManager.Instance.Pause();

        var modal = new ModalUIData
        {
            Type = ModalType.OkCancel,
            Title = "정말 다 하셨어요?",
            Desc = "공부 스케줄을 종료합니다.",
            OkBtnText = "종료",
            CancelBtnText = "계속 공부",
            OkAction = () =>
            {
                // 최종 완료 처리
                LobbyManager.Instance.IsComplete = true;
                Logger.Log($"{GetType()}::공부 완료 확인됨");
            },
            CancelAction = () =>
            {
                // 일시정지 상태 해제 및 타이머 재시작 (체크박스 상태는 유지)
                Resume(pauseStartTime);
                LobbyManager.Instance.Resume();
                LobbyManager.Instance.IsComplete = false;
                
                // 타이머 재시작
                TimerStart();
                
                Logger.Log($"{GetType()}::공부 계속하기 선택됨 - 타이머 재시작, 체크박스 상태 유지");
            }
        };

        UIManager.Instance.OpenUI<ModalUI>(modal);
    }
    
    /// <summary>
    /// 외부에서 호출할 수 있는 완료 상태 업데이트 메서드
    /// StudyingItemSlot에서 체크박스 상태 변경 시 호출
    /// </summary>
    public void OnStudyItemCheckChanged()
    {
        CheckAndUpdateCompletionStatus();
    }

    public bool CheckCompleted()
    {
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData == null)
        {
            Logger.LogError($"{GetType()}::UserStudyData is null");
            return false;
        }

        if (userStudyData.StudyItemDataList == null || userStudyData.StudyItemDataList.Count == 0)
        {
            Logger.LogWarning($"{GetType()}::StudyItemDataList is null or empty");
            return false;
        }

        bool allCompleted = userStudyData.StudyItemDataList.TrueForAll(item => item.check);
        
        Logger.Log($"{GetType()}::완료 상태 체크 결과: {allCompleted} (총 {userStudyData.StudyItemDataList.Count}개 중 {userStudyData.StudyItemDataList.FindAll(item => item.check).Count}개 완료)");
        
        return allCompleted;
    }

    protected override void OnDestroy()
    {
        StopAllCoroutines();
        
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnCompleteChanged -= UpdateCompleteButton;
    }
    
    private void OnDisable()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
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

        userTimeData.Time += (long) _elapsedTime;
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
        
        dailyItem.Time += (long) _elapsedTime;
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
