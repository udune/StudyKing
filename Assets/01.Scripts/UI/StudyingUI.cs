using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    private DateTime _sessionStartTime;  // 세션 시작 시간
    private DateTime _lastUpdateTime;    // 마지막 UI 업데이트 시간
    private DateTime _lastCheckTime;     // 마지막 체크 시간 (deprecated)
    private float _totalPausedSeconds;   // 총 일시정지 시간
    
    // 과목별 시작 시간 관리
    private Dictionary<string, DateTime> _currentSubjectStartTimes = new Dictionary<string, DateTime>();
    
    // 세션 시작 시 과목별 초기 시간 기록 (세션별 실제 증가분 계산용)
    private Dictionary<string, long> _sessionStartSubjectTimes = new Dictionary<string, long>();
    
    // 현재 활성 과목 추적 (체크되지 않은 과목 중 첫 번째)
    private string _currentActiveSubject = null;
    private DateTime _currentActiveSubjectStartTime;
    
    // 호환성을 위해 기존 프로퍼티 유지 (deprecated)
    public DateTime StartTime => _sessionStartTime;

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

        var now = DateTime.UtcNow;
        _sessionStartTime = now;
        _lastUpdateTime = now;
        _lastCheckTime = now;
        _totalPausedSeconds = 0f;
        
        // 과목별 시작 시간 초기화
        ClearAllSubjectStartTimes();
        
        // 세션 시작 시 과목별 초기 시간 기록
        InitializeSessionStartSubjectTimes();
        
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
        
        // 실시간 과목별 시간 추적 시작
        UpdateCurrentActiveSubject();
        StartCoroutine(ActiveSubjectTimeCoroutine());

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
            _lastUpdateTime = DateTime.UtcNow;
            float elapsedSeconds = GetElapsedSeconds();
            
            int hours = Mathf.FloorToInt(elapsedSeconds / 3600);
            int minutes = Mathf.FloorToInt((elapsedSeconds % 3600) / 60);
            int seconds = Mathf.FloorToInt(elapsedSeconds % 60);

            time.text = hours > 0 ? $"{hours:00}:{minutes:00}:{seconds:00}" : $"{minutes:00}:{seconds:00}";

            yield return null;
        }
    }

    public void Resume(DateTime paused)
    {
        var pausedDuration = (float)(DateTime.UtcNow - paused).TotalSeconds;
        _totalPausedSeconds += pausedDuration;
        
        // 마지막 체크 시간도 일시정지 시간만큼 조정 (deprecated)
        _lastCheckTime = _lastCheckTime.AddSeconds(pausedDuration);
        
        // 진행 중인 모든 과목의 시작 시간을 일시정지 시간만큼 조정
        var adjustedStartTimes = new Dictionary<string, DateTime>();
        foreach (var kvp in _currentSubjectStartTimes)
        {
            adjustedStartTimes[kvp.Key] = kvp.Value.AddSeconds(pausedDuration);
        }
        _currentSubjectStartTimes = adjustedStartTimes;
        
        Logger.Log($"{GetType()}::Resume - 일시정지 시간: {pausedDuration:F1}초, 총 일시정지: {_totalPausedSeconds:F1}초, 진행중인 과목 {_currentSubjectStartTimes.Count}개 시간 조정");
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
                // 마지막 체크된 항목 하나만 해제
                var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
                if (userStudyData?.StudyItemDataList != null)
                {
                    // 마지막으로 체크된 항목 찾아서 해제
                    for (int i = userStudyData.StudyItemDataList.Count - 1; i >= 0; i--)
                    {
                        if (userStudyData.StudyItemDataList[i].check)
                        {
                            userStudyData.StudyItemDataList[i].check = false;
                            userStudyData.SaveData();
                            break; // 하나만 해제하고 중단
                        }
                    }
                }
                
                // 완료 상태를 먼저 false로 설정하여 Resume이 동작하도록 함
                LobbyManager.Instance.IsComplete = false;
                
                // 일시정지 상태 해제 및 타이머 재시작
                LobbyManager.Instance.Resume();
                Resume(pauseStartTime);
                
                // 스크롤 리스트 데이터 새로고침 (체크박스 상태 반영)
                RefreshScrollListFromUserData();
                
                Logger.Log($"{GetType()}::공부 계속하기 선택됨 - 마지막 항목 해제, 타이머 재시작");
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
    
    /// <summary>
    /// 순 경과 시간을 반환 (일시정지 시간 제외, 초 단위)
    /// </summary>
    public float GetElapsedSeconds()
    {
        var totalElapsed = (float)(DateTime.UtcNow - _sessionStartTime).TotalSeconds;
        return totalElapsed - _totalPausedSeconds;
    }
    
    /// <summary>
    /// 현재까지의 경과 시간을 반환 (호환성을 위해 유지)
    /// </summary>
    public float GetCurrentElapsedTime()
    {
        return GetElapsedSeconds();
    }
    
    /// <summary>
    /// 마지막 체크 시간을 반환
    /// </summary>
    public DateTime GetLastCheckTime()
    {
        return _lastCheckTime;
    }
    
    /// <summary>
    /// 마지막 체크 시간을 설정 (deprecated - 호환성용)
    /// </summary>
    public void SetLastCheckTime(DateTime time)
    {
        _lastCheckTime = time;
    }
    
    /// <summary>
    /// 특정 과목의 시작 시간을 설정
    /// </summary>
    public void SetCurrentSubjectStartTime(string subjectName, DateTime startTime)
    {
        _currentSubjectStartTimes[subjectName] = startTime;
        Logger.Log($"{GetType()}::과목 '{subjectName}' 시작시간 설정: {startTime:HH:mm:ss}");
    }
    
    /// <summary>
    /// 특정 과목의 시작 시간을 반환
    /// </summary>
    public DateTime? GetCurrentSubjectStartTime(string subjectName)
    {
        return _currentSubjectStartTimes.TryGetValue(subjectName, out DateTime startTime) ? startTime : null;
    }
    
    /// <summary>
    /// 특정 과목의 시작 시간을 초기화
    /// </summary>
    public void ClearCurrentSubjectStartTime(string subjectName)
    {
        if (_currentSubjectStartTimes.Remove(subjectName))
        {
            Logger.Log($"{GetType()}::과목 '{subjectName}' 시작시간 초기화");
        }
    }
    
    /// <summary>
    /// 모든 과목의 시작 시간을 초기화
    /// </summary>
    public void ClearAllSubjectStartTimes()
    {
        _currentSubjectStartTimes.Clear();
        Logger.Log($"{GetType()}::모든 과목 시작시간 초기화");
    }
    
    /// <summary>
    /// 세션 시작 시 과목별 초기 시간 기록 (세션별 실제 증가분 계산용)
    /// </summary>
    private void InitializeSessionStartSubjectTimes()
    {
        _sessionStartSubjectTimes.Clear();
        
        var userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
        if (userSubjectTimeData?.SubjectTimeItemDataList != null)
        {
            foreach (var subject in userSubjectTimeData.SubjectTimeItemDataList)
            {
                _sessionStartSubjectTimes[subject.Name] = subject.Time;
                Logger.Log($"{GetType()}::세션 시작 기록 - {subject.Name}: {subject.Time}초");
            }
            Logger.Log($"{GetType()}::세션 시작 시 과목별 시간 기록 완료 - {_sessionStartSubjectTimes.Count}개 과목");
        }
        else
        {
            Logger.LogWarning($"{GetType()}::userSubjectTimeData 또는 SubjectTimeItemDataList가 null");
        }
    }
    
    /// <summary>
    /// 진행 중인 모든 과목의 시작 시간 초기화 (시간 계산은 StudyingItemSlot에서만 처리)
    /// </summary>
    private void FinishAllOngoingSubjects()
    {
        if (_currentSubjectStartTimes.Count == 0)
        {
            Logger.Log($"{GetType()}::진행 중인 과목이 없음");
            return;
        }
        
        // 진행 중인 과목들의 시작 시간만 초기화 (시간 누적은 StudyingItemSlot에서 처리됨)
        int ongoingCount = _currentSubjectStartTimes.Count;
        ClearAllSubjectStartTimes();
        
        Logger.Log($"{GetType()}::진행 중이던 {ongoingCount}개 과목 시작시간 초기화 완료 (시간 누적은 StudyingItemSlot에서 처리됨)");
    }
    
    /// <summary>
    /// 체크박스를 해제하지 않고 공부 완료한 과목들의 시간을 계산 (StudyingItemSlot에서 처리되지 않은 시간만)
    /// </summary>
    private void CalculateUnfinishedSubjectsTime()
    {
        if (_currentSubjectStartTimes.Count == 0)
        {
            Logger.Log($"{GetType()}::체크박스 해제되지 않은 진행 중인 과목이 없음");
            return;
        }
        
        var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        if (userSubjectTimeData == null)
        {
            Logger.LogWarning($"{GetType()}::UserSubjectTimeData가 null");
            return;
        }
        
        var currentTime = DateTime.UtcNow;
        int calculatedCount = 0;
        
        foreach (var kvp in _currentSubjectStartTimes)
        {
            var subjectName = kvp.Key;
            var startTime = kvp.Value;
            var studyDuration = (currentTime - startTime).TotalSeconds;
            
            var subject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault(x => x.Name.Equals(subjectName));
            if (subject != null)
            {
                long beforeTime = subject.Time;
                subject.Time += (long)studyDuration;
                long afterTime = subject.Time;
                
                Logger.Log($"{GetType()}::미완료 과목 '{subjectName}' 시간 계산 - 이전: {beforeTime}초, 추가: {studyDuration:F1}초, 이후: {afterTime}초");
                calculatedCount++;
            }
        }
        
        if (calculatedCount > 0)
        {
            userSubjectTimeData.SaveData();
            Logger.Log($"{GetType()}::체크박스 해제되지 않은 {calculatedCount}개 과목 시간 계산 완료");
        }
    }
    
    /// <summary>
    /// 시간 데이터 일관성 검증
    /// </summary>
    private void ValidateTimeConsistency()
    {
        try
        {
            var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
            if (userSubjectTimeData?.SubjectTimeItemDataList == null)
                return;

            var totalSubjectTime = userSubjectTimeData.SubjectTimeItemDataList.Sum(x => x.Time);
            var totalSessionTime = (long)GetElapsedSeconds();
            var unclassifiedTime = totalSessionTime - totalSubjectTime;

            Logger.Log($"{GetType()}::시간 검증 - 과목별 합계: {totalSubjectTime}초, 세션 시간: {totalSessionTime}초, 미분류 시간: {unclassifiedTime}초");

            // 과목별 시간이 세션 시간보다 많으면 문제
            if (totalSubjectTime > totalSessionTime + 5) // 5초 오차 허용
            {
                Logger.LogWarning($"{GetType()}::과목별 시간이 세션시간을 초과 - 과목별: {totalSubjectTime}초, 세션: {totalSessionTime}초");
            }
            
            // 진행중인 과목이 있으면 알림
            if (_currentSubjectStartTimes.Count > 0)
            {
                Logger.Log($"{GetType()}::진행 중인 과목: {string.Join(", ", _currentSubjectStartTimes.Keys)}");
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::시간 검증 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 시간 데이터 강제 동기화 (필요시 사용)
    /// </summary>
    public void SynchronizeTimeData()
    {
        try
        {
            var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
            if (userSubjectTimeData?.SubjectTimeItemDataList == null)
                return;

            var sessionTime = (long)GetElapsedSeconds();
            var totalSubjectTime = userSubjectTimeData.SubjectTimeItemDataList.Sum(x => x.Time);
            
            if (totalSubjectTime == 0 && sessionTime > 0)
            {
                // 과목별 시간이 없는데 세션 시간이 있는 경우, 기본 과목에 할당
                var defaultSubject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault();
                if (defaultSubject != null)
                {
                    defaultSubject.Time = sessionTime;
                    userSubjectTimeData.SaveData();
                    Logger.Log($"{GetType()}::시간 동기화 - {sessionTime}초를 '{defaultSubject.Name}'에 할당");
                }
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::시간 동기화 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// UserData를 기반으로 스크롤 리스트의 체크박스 상태를 새로고침
    /// </summary>
    private void RefreshScrollListFromUserData()
    {
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData?.StudyItemDataList == null) return;

        for (int i = 0; i < studyingScrollList.GetDataCount(); i++)
        {
            var slotData = studyingScrollList.GetData(i) as StudyingItemSlotData;
            if (slotData == null) continue;

            var userData = userStudyData.StudyItemDataList.Find(x => x.id == slotData.Id);
            if (userData != null)
            {
                slotData.Check = userData.check;
                studyingScrollList.UpdateData(slotData);
            }
        }
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

        var sessionElapsedSeconds = (long)GetElapsedSeconds();
        
        // 체크박스를 해제하지 않고 공부 완료한 과목들의 시간을 계산
        CalculateUnfinishedSubjectsTime();
        
        // 진행 중인 모든 과목의 시작 시간 초기화
        FinishAllOngoingSubjects();
        
        // 세션별 실제 공부 시간 증가분 계산 (전체 합계가 아닌 세션에서 실제로 증가한 시간만)
        var userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        long sessionStudyTimeIncrease = 0;
        
        Logger.Log($"{GetType()}::세션 증가분 계산 시작 - _sessionStartSubjectTimes.Count: {_sessionStartSubjectTimes.Count}");
        
        if (userSubjectTimeData?.SubjectTimeItemDataList != null)
        {
            Logger.Log($"{GetType()}::과목별 시간 데이터 개수: {userSubjectTimeData.SubjectTimeItemDataList.Count}");
            
            foreach (var subject in userSubjectTimeData.SubjectTimeItemDataList)
            {
                // 세션 시작 시 기록된 시간과 현재 시간의 차이만 계산
                long startTime = _sessionStartSubjectTimes.ContainsKey(subject.Name) 
                    ? _sessionStartSubjectTimes[subject.Name] : 0;
                long timeIncrease = subject.Time - startTime;
                sessionStudyTimeIncrease += timeIncrease;
                
                Logger.Log($"{GetType()}::과목 '{subject.Name}' 세션 증가분: {timeIncrease}초 (시작: {startTime}초, 현재: {subject.Time}초)");
            }
            
            Logger.Log($"{GetType()}::총 세션 증가분: {sessionStudyTimeIncrease}초");
        }
        else
        {
            Logger.LogWarning($"{GetType()}::userSubjectTimeData 또는 SubjectTimeItemDataList가 null - 증가분: 0초");
        }
        
        // 시간 데이터 일관성 검증
        ValidateTimeConsistency();
        
        // 세션별 실제 증가분만 총 시간에 추가
        long beforeTime = userTimeData.Time;
        userTimeData.Time += sessionStudyTimeIncrease;
        long afterTime = userTimeData.Time;
        
        Logger.Log($"{GetType()}::총 시간 업데이트 - 이전: {beforeTime}초, 증가분: {sessionStudyTimeIncrease}초, 이후: {afterTime}초");
        
        userTimeData.SaveData();
        
        // TimeUI 즉시 업데이트
        UIManager.Instance.RefreshTimeUI();
        
        Logger.Log($"{GetType()}::OnClickFinishStudyItem - 세션 시간: {sessionElapsedSeconds}초, 세션 공부시간 증가분: {sessionStudyTimeIncrease}초");

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
        
        // 일별 시간도 세션별 증가분으로 기록
        dailyItem.Time += sessionStudyTimeIncrease;
        userDailyTimeData.SaveData();
        
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "time", userTimeData.Time.ToString() }
        };
        FirebaseManager.Instance.LogCustomEvent("study_clear", parameters);
        
        UIManager.Instance.SetTimeUIVisible(true);
        UIManager.Instance.CloseAllOpenUI();
    }
    
    /// <summary>
    /// 현재 활성 과목 업데이트 (체크되지 않은 첫 번째 과목)
    /// </summary>
    public void UpdateCurrentActiveSubject()
    {
        var userStudyData = UserDataManager.Instance?.GetUserData<UserStudyData>();
        if (userStudyData?.StudyItemDataList == null) return;
        
        string newActiveSubject = null;
        
        // 체크되지 않은 첫 번째 과목 찾기
        foreach (var item in userStudyData.StudyItemDataList)
        {
            if (!item.check)
            {
                newActiveSubject = item.name;
                break;
            }
        }
        
        // 활성 과목 변경 시 이전 과목의 시간 저장
        if (_currentActiveSubject != newActiveSubject)
        {
            if (_currentActiveSubject != null)
            {
                SaveCurrentActiveSubjectTime();
            }
            
            _currentActiveSubject = newActiveSubject;
            if (_currentActiveSubject != null)
            {
                _currentActiveSubjectStartTime = DateTime.UtcNow;
                Logger.Log($"{GetType()}::활성 과목 변경: '{_currentActiveSubject}' 시작");
            }
            else
            {
                Logger.Log($"{GetType()}::모든 과목이 완료됨 - 활성 과목 없음");
            }
        }
    }
    
    /// <summary>
    /// 현재 활성 과목의 시간을 저장
    /// </summary>
    private void SaveCurrentActiveSubjectTime()
    {
        if (_currentActiveSubject == null) return;
        
        var userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
        if (userSubjectTimeData == null) return;
        
        var currentTime = DateTime.UtcNow;
        var studyDuration = (currentTime - _currentActiveSubjectStartTime).TotalSeconds;
        
        var subject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault(x => x.Name.Equals(_currentActiveSubject));
        if (subject != null)
        {
            long beforeTime = subject.Time;
            subject.Time += (long)studyDuration;
            long afterTime = subject.Time;
            
            userSubjectTimeData.SaveData();
            Logger.Log($"{GetType()}::활성 과목 '{_currentActiveSubject}' 시간 저장 - 이전: {beforeTime}초, 추가: {studyDuration:F1}초, 이후: {afterTime}초");
        }
    }
    
    /// <summary>
    /// 활성 과목 실시간 시간 추적 코루틴
    /// </summary>
    private IEnumerator ActiveSubjectTimeCoroutine()
    {
        while (!LobbyManager.Instance.IsPaused && !LobbyManager.Instance.IsComplete)
        {
            yield return new WaitForSeconds(1f); // 1초마다 업데이트
            
            if (_currentActiveSubject != null)
            {
                var userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
                if (userSubjectTimeData != null)
                {
                    var currentTime = DateTime.UtcNow;
                    var studyDuration = (currentTime - _currentActiveSubjectStartTime).TotalSeconds;
                    
                    var subject = userSubjectTimeData.SubjectTimeItemDataList.FirstOrDefault(x => x.Name.Equals(_currentActiveSubject));
                    if (subject != null)
                    {
                        // 실시간으로 시간 업데이트 (저장은 체크 변경 시에만)
                        Logger.Log($"{GetType()}::'{_currentActiveSubject}' 진행중: {studyDuration:F0}초");
                    }
                }
            }
            else
            {
                // 모든 과목이 완료되면 코루틴 종료
                break;
            }
        }
    }
}
