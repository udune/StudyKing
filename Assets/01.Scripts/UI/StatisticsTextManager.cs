using System;
using System.Linq;
using System.Text;
using _01.Scripts.Manager;
using Common;
using TMPro;

/// <summary>
/// 대시보드의 통계 텍스트 업데이트를 관리하는 클래스
/// </summary>
public class StatisticsTextManager
{
    private readonly TMP_Text _totalTimeText;
    private readonly TMP_Text _weeklyTotalTimeText;
    private readonly TMP_Text _subjectTimeText;
    private readonly ErrorHandler _errorHandler;
    private readonly Action _retryCallback;

    private readonly StringBuilder _sbSubject = new StringBuilder();

    public StatisticsTextManager(TMP_Text totalTimeText, TMP_Text weeklyTotalTimeText, TMP_Text subjectTimeText, ErrorHandler errorHandler, Action retryCallback)
    {
        _totalTimeText = totalTimeText;
        _weeklyTotalTimeText = weeklyTotalTimeText;
        _subjectTimeText = subjectTimeText;
        _errorHandler = errorHandler;
        _retryCallback = retryCallback;
    }

    /// <summary>
    /// 모든 통계 텍스트를 새로고침합니다.
    /// </summary>
    public void RefreshAllStatistics()
    {
        RefreshTotalTime();
        RefreshWeeklyTime();
        RefreshSubjectTime();
    }

    /// <summary>
    /// 총 학습 시간 텍스트를 업데이트합니다.
    /// </summary>
    private void RefreshTotalTime()
    {
        if (_totalTimeText == null) return;

        var userData = UserDataManager.Instance.GetUserData<UserTimeData>();
        if (userData == null)
        {
            _errorHandler?.Show(ErrorType.DataError, _retryCallback);
            return;
        }

        _totalTimeText.text = FormatStudyTime(userData.Time);
    }

    /// <summary>
    /// 주간 총 학습 시간 텍스트를 업데이트합니다.
    /// </summary>
    private void RefreshWeeklyTime()
    {
        if (_weeklyTotalTimeText == null) return;
        
        var dailyData = UserDataManager.Instance.GetUserData<UserDailyTimeData>();
        if (dailyData == null)
        {
            _errorHandler?.Show(ErrorType.DataError, _retryCallback);
            return;
        }

        long weeklyTotal = CalculateWeeklyTotal(dailyData);
        _weeklyTotalTimeText.text = FormatStudyTime(weeklyTotal);
    }

    /// <summary>
    /// 과목별 학습 시간 텍스트를 업데이트합니다.
    /// </summary>
    private void RefreshSubjectTime()
    {
        if (_subjectTimeText == null) return;
        
        var subjectData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        if (subjectData == null)
        {
            _errorHandler?.Show(ErrorType.DataError, _retryCallback);
            return;
        }

        _sbSubject.Clear();

        // 시간을 기준으로 내림차순 정렬하여 상위 과목부터 표시
        foreach (var item in subjectData.SubjectTimeItemDataList.OrderByDescending(x => x.Time))
        {
            _sbSubject.AppendLine($"{item.Name}: {FormatStudyTime(item.Time)}");
        }
        
        _subjectTimeText.text = _sbSubject.ToString();
    }
    
    /// <summary>
    /// 초를 "X시간 Y분" 또는 "Y분" 형식의 문자열로 변환합니다.
    /// </summary>
    private string FormatStudyTime(long totalSeconds)
    {
        long hours = totalSeconds / 3600;
        long minutes = (totalSeconds % 3600) / 60;

        if (hours > 0)
        {
            return $"{hours}시간 {minutes}분";
        }
        
        return $"{minutes}분";
    }

    /// <summary>
    /// 최근 7일간의 총 학습 시간을 계산합니다.
    /// </summary>
    private long CalculateWeeklyTotal(UserDailyTimeData dailyData)
    {
        DateTime now = DateTime.UtcNow.AddHours(9); // KST
        long weeklyTotal = 0;

        for (int i = 0; i < 7; i++)
        {
            string date = now.AddDays(-i).ToString("yyyy-MM-dd");
            var dayData = dailyData.DailyTimeItemDataList.FirstOrDefault(x => x.Date == date);
            if (dayData != null)
            {
                weeklyTotal += dayData.Time;
            }
        }
        
        return weeklyTotal;
    }
}
