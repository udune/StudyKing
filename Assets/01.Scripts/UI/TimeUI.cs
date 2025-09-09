using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private StringBuilder sb = new StringBuilder();
    private UserTimeData cachedTimeData;
    private long lastDisplayTime = -1;
    
    private void OnEnable()
    {
        // 초기 데이터 로드
        RefreshTimeData();
        
        // UserDataManager가 데이터를 업데이트할 때 알림받기 (이벤트 구독 방식이 있다면)
        // 또는 주기적으로 체크하되 훨씬 긴 간격으로 (예: 10초)
        InvokeRepeating(nameof(CheckForTimeUpdate), 1f, 10f); // 10초마다 체크
        
        Logger.Log($"{GetType()}::Started time monitoring");
    }
    
    private void OnDisable()
    {
        CancelInvoke();
        Logger.Log($"{GetType()}::Stopped time monitoring");
    }
    
    private void RefreshTimeData()
    {
        if (UserDataManager.Instance == null)
        {
            Logger.LogWarning($"{GetType()}::UserDataManager.Instance is null");
            return;
        }
        
        cachedTimeData = UserDataManager.Instance.GetUserData<UserTimeData>();
        
        if (cachedTimeData != null)
        {
            Logger.Log($"{GetType()}::RefreshTimeData - 로드된 Time: {cachedTimeData.Time}초");
        }
        else
        {
            Logger.LogWarning($"{GetType()}::RefreshTimeData - cachedTimeData is null");
        }
        
        SetValue();
    }
    
    private void CheckForTimeUpdate()
    {
        // UserTimeData가 변경되었는지만 체크
        if (cachedTimeData != null)
        {
            var currentTimeData = UserDataManager.Instance?.GetUserData<UserTimeData>();
            if (currentTimeData != null && currentTimeData.Time != lastDisplayTime)
            {
                cachedTimeData = currentTimeData;
                SetValue();
            }
        }
        else
        {
            RefreshTimeData();
        }
    }

    public void SetValue()
    {
        if (cachedTimeData == null)
        {
            Logger.LogWarning($"{GetType()}::No cached UserTimeData found");
            if (timeText != null)
                timeText.text = "데이터 없음";
            return;
        }
        
        Logger.Log($"{GetType()}::SetValue 호출 - cachedTimeData.Time: {cachedTimeData.Time}초");
        
        string formattedTime = CalculateTimeFormat(cachedTimeData.Time);
        lastDisplayTime = cachedTimeData.Time;
        
        Logger.Log($"{GetType()}::Updating time display: {cachedTimeData.Time}s -> {formattedTime}");
        
        if (timeText != null)
        {
            timeText.text = formattedTime;
            Logger.Log($"{GetType()}::timeText 업데이트 완료: '{formattedTime}'");
        }
        else
        {
            Logger.LogError($"{GetType()}::timeText is null");
        }
    }

    private string CalculateTimeFormat(long time)
    {
        sb.Clear();
        
        int hour = (int)(time / 3600);
        int minute = (int)((time % 3600) / 60);
        int second = (int)(time % 60);

        if (hour > 0) sb.Append(hour).Append("시간 ");
        if (minute > 0) sb.Append(minute).Append("분 ");
        if (second > 0) sb.Append(second).Append("초 ");

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "0초";
    }
    
    /// <summary>
    /// 외부에서 시간 업데이트를 요청할 때 사용
    /// 예: 공부 시간이 증가했을 때, 데이터를 저장한 후 호출
    /// </summary>
    public void ForceRefresh()
    {
        RefreshTimeData();
        Logger.Log($"{GetType()}::Force refreshed time display");
    }
}
