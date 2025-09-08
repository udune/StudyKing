using System;
using UnityEngine;
using Logger = Common.Logger;

/// <summary>
/// 공부 중 일시정지 UI 클래스
/// 공부를 일시정지했을 때 재개 또는 종료를 선택할 수 있습니다
/// </summary>
public class PauseUI : BaseUI
{
    [Header("일시정지 정보")]
    [SerializeField] private TMPro.TMP_Text pauseTimeText;     // 일시정지 시간을 표시하는 텍스트
    [SerializeField] private TMPro.TMP_Text pauseMessageText; // 일시정지 메시지 텍스트
    
    // 일시정지 시작 시간
    private DateTime _pauseStartTime;
    
    // 일시정지 시간 업데이트용 코루틴
    private Coroutine _updateTimeCoroutine;

    /// <summary>
    /// UI가 활성화될 때 호출되는 함수
    /// </summary>
    protected override void OnShow()
    {
        base.OnShow();
        
        // 일시정지 시작 시간 기록
        _pauseStartTime = DateTime.UtcNow;
        
        // UI 설정
        SetupPauseUI();
        
        // 시간 업데이트 시작
        StartTimeUpdate();
        
        Logger.Log($"{GetType()}::일시정지 UI가 표시되었습니다 - 시작 시간: {_pauseStartTime}");
    }
    
    /// <summary>
    /// 일시정지 UI를 설정하는 함수
    /// </summary>
    private void SetupPauseUI()
    {
        // 일시정지 메시지 설정
        if (pauseMessageText != null)
        {
            pauseMessageText.text = "잠시 휴식 중입니다 ☕\n언제든 다시 시작할 수 있어요!";
        }
        
        // 초기 시간 표시
        UpdatePauseTimeDisplay();
    }
    
    /// <summary>
    /// 시간 업데이트를 시작하는 함수
    /// </summary>
    private void StartTimeUpdate()
    {
        if (_updateTimeCoroutine != null)
        {
            StopCoroutine(_updateTimeCoroutine);
        }
        
        _updateTimeCoroutine = StartCoroutine(UpdateTimeCoroutine());
    }
    
    /// <summary>
    /// 시간을 업데이트하는 코루틴
    /// </summary>
    private System.Collections.IEnumerator UpdateTimeCoroutine()
    {
        while (gameObject.activeInHierarchy)
        {
            UpdatePauseTimeDisplay();
            yield return new WaitForSeconds(1f); // 1초마다 업데이트
        }
    }
    
    /// <summary>
    /// 일시정지 시간을 화면에 표시하는 함수
    /// </summary>
    private void UpdatePauseTimeDisplay()
    {
        if (pauseTimeText == null) return;
        
        try
        {
            TimeSpan pauseDuration = DateTime.UtcNow - _pauseStartTime;
            string timeText = FormatTimeSpan(pauseDuration);
            pauseTimeText.text = $"일시정지 시간: {timeText}";
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::시간 표시 업데이트 중 오류: {e.Message}");
            pauseTimeText.text = "일시정지 시간: --:--";
        }
    }
    
    /// <summary>
    /// TimeSpan을 읽기 쉬운 형태로 포맷하는 함수
    /// </summary>
    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else
        {
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }

    /// <summary>
    /// 재개 버튼 클릭 시 호출되는 함수
    /// </summary>
    public void OnClickResume()
    {
        Logger.Log($"{GetType()}::재개 버튼이 클릭되었습니다");
        
        try
        {
            // 공부 중 UI에 일시정지 시간 전달
            ResumeStudySession();
            
            // 로비 매니저에 재개 알림
            ResumeGameManager();
            
            // 일시정지 UI 닫기
            CloseUI();
            
            Logger.Log($"{GetType()}::공부 재개 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::재개 처리 중 오류 발생: {e.Message}");
            ShowErrorModal("오류", "재개하는데 문제가 발생했습니다.");
        }
    }
    
    /// <summary>
    /// 공부 세션을 재개하는 함수
    /// </summary>
    private void ResumeStudySession()
    {
        var studyingUI = UIManager.Instance?.GetActiveUI<StudyingUI>() as StudyingUI;
        if (studyingUI != null)
        {
            // StudyingUI에 일시정지 시작 시간을 전달하여 재개 처리
            studyingUI.Resume(_pauseStartTime);
            Logger.Log($"{GetType()}::StudyingUI에 재개 신호를 보냈습니다");
        }
        else
        {
            Logger.LogWarning($"{GetType()}::StudyingUI를 찾을 수 없습니다");
        }
    }
    
    /// <summary>
    /// 게임 매니저를 재개하는 함수
    /// </summary>
    private void ResumeGameManager()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.Resume();
            Logger.Log($"{GetType()}::LobbyManager 재개 완료");
        }
        else
        {
            Logger.LogWarning($"{GetType()}::LobbyManager를 찾을 수 없습니다");
        }
    }

    /// <summary>
    /// 종료 버튼 클릭 시 호출되는 함수
    /// </summary>
    public void OnClickQuit()
    {
        Logger.Log($"{GetType()}::종료 버튼이 클릭되었습니다");
        
        try
        {
            // 종료 확인 모달 표시
            ShowQuitConfirmationModal();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::종료 처리 중 오류 발생: {e.Message}");
        }
    }
    
    /// <summary>
    /// 종료 확인 모달을 표시하는 함수
    /// </summary>
    private void ShowQuitConfirmationModal()
    {
        var modalData = new ModalUIData
        {
            Type = ModalType.OkCancel,
            Title = "공부 종료",
            Desc = "정말 공부를 종료하시겠어요?\n지금까지 기록한 시간은 사라져요.",
            OkBtnText = "종료",
            CancelBtnText = "계속 공부",
            OkAction = OnConfirmQuit,
            CancelAction = OnCancelQuit
        };
        
        UIManager.Instance?.OpenUI<ModalUI>(modalData);
        Logger.Log($"{GetType()}::종료 확인 모달을 표시했습니다");
    }
    
    /// <summary>
    /// 종료 확인 시 호출되는 함수
    /// </summary>
    private void OnConfirmQuit()
    {
        Logger.Log($"{GetType()}::공부 종료 확인됨");
        
        try
        {
            // 로비 매니저에 완료되지 않음을 알림
            if (LobbyManager.Instance != null)
            {
                LobbyManager.Instance.IsComplete = false;
            }
            
            // 공부 중 UI 닫기
            CloseStudyingUI();
            
            // 일시정지 UI 닫기
            CloseUI();
            
            Logger.Log($"{GetType()}::공부 종료 처리 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::공부 종료 처리 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 종료 취소 시 호출되는 함수
    /// </summary>
    private void OnCancelQuit()
    {
        Logger.Log($"{GetType()}::공부 종료가 취소되었습니다");
        // 별도 처리 없이 모달만 닫힘
    }
    
    /// <summary>
    /// 공부 중 UI를 닫는 함수
    /// </summary>
    private void CloseStudyingUI()
    {
        var studyingUI = UIManager.Instance?.GetActiveUI<StudyingUI>();
        if (studyingUI != null)
        {
            studyingUI.CloseUI(true); // 강제 닫기
            Logger.Log($"{GetType()}::StudyingUI를 닫았습니다");
        }
        else
        {
            Logger.LogWarning($"{GetType()}::닫을 StudyingUI를 찾을 수 없습니다");
        }
    }
    
    /// <summary>
    /// 에러 모달을 표시하는 함수
    /// </summary>
    private void ShowErrorModal(string title, string message)
    {
        try
        {
            var modalData = new ModalUIData
            {
                Type = ModalType.Ok,
                Title = title,
                Desc = message,
                OkBtnText = "확인"
            };
            
            UIManager.Instance?.OpenUI<ModalUI>(modalData);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::에러 모달 표시 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// UI가 닫힐 때 호출되는 함수
    /// </summary>
    protected override void OnClose()
    {
        try
        {
            // 시간 업데이트 코루틴 정지
            StopTimeUpdate();
            
            Logger.Log($"{GetType()}::PauseUI가 정리되었습니다");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::OnClose 중 오류: {e.Message}");
        }
        finally
        {
            base.OnClose();
        }
    }
    
    /// <summary>
    /// 시간 업데이트를 정지하는 함수
    /// </summary>
    private void StopTimeUpdate()
    {
        if (_updateTimeCoroutine != null)
        {
            StopCoroutine(_updateTimeCoroutine);
            _updateTimeCoroutine = null;
        }
    }
    
    /// <summary>
    /// 뒤로가기 키 처리 (재개로 처리)
    /// </summary>
    protected override void OnBackKeyPressed()
    {
        Logger.Log($"{GetType()}::뒤로가기 키 - 공부 재개");
        OnClickResume();
    }
    
    /// <summary>
    /// 일시정지 시작 시간을 반환하는 함수 (디버깅용)
    /// </summary>
    public DateTime GetPauseStartTime()
    {
        return _pauseStartTime;
    }
    
    /// <summary>
    /// 현재 일시정지 시간을 반환하는 함수 (디버깅용)
    /// </summary>
    public TimeSpan GetCurrentPauseDuration()
    {
        return DateTime.UtcNow - _pauseStartTime;
    }
}