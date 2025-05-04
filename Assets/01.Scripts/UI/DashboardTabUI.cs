using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class DashboardTabUI : BaseUI
{
    [SerializeField] private TMP_Text totalTime;
    [SerializeField] private TMP_Text weeklyTotalTime;
    // [SerializeField] private TMP_Text subjectTime;
    [SerializeField] private TMP_Text weeklyTime;
    [SerializeField] private TMP_Text monthlyTime;
    
    Dictionary<string, long> last7Days = new Dictionary<string, long>();
    Dictionary<string, long> last30Days = new Dictionary<string, long>();
    
    private StringBuilder sb = new StringBuilder();
    private StringBuilder sb_subject = new StringBuilder();
    private StringBuilder sb_weekly = new StringBuilder();
    private StringBuilder sb_monthly = new StringBuilder();
    
    private void OnEnable()
    {
        SetTotalTime();
        SetWeeklyTime();
        SetSubjectTime();
    }

    private void SetTotalTime()
    {
        UserTimeData userTimeData = UserDataManager.Instance.GetUserData<UserTimeData>();
        if (userTimeData == null)
        {
            Logger.Log($"{GetType()}::UserTimeData is null");
            return;
        }
        
        totalTime.text = CalculateTimeFormat(userTimeData.Time);
    }

    private void SetWeeklyTime()
    {
        UserDailyTimeData userDailyTimeData = UserDataManager.Instance.GetUserData<UserDailyTimeData>();
        if (userDailyTimeData == null)
        {
            Logger.Log($"{GetType()}::UserDailyTimeData is null");
            return;
        }
        
        long weeklyTotalTime = 0;
        DateTime today = DateTime.UtcNow.AddHours(9);
        DateTime monday = today.AddDays(-(int)today.DayOfWeek + 1);
        foreach (var dailyTime in userDailyTimeData.DailyTimeItemDataList)
        {
            if (DateTime.TryParse(dailyTime.Date, out var date) && date >= monday && date <= today)
            {
                weeklyTotalTime += dailyTime.Time;
            }
        }

        sb_weekly.Clear();
        for (int i = 6; i >= 0; i--)
        {
            DateTime day = today.AddDays(-i);
            string label = day.ToString("ddd");
            string date = day.ToString("yyyy-MM-dd");
            
            DailyTimeItemData data = userDailyTimeData.DailyTimeItemDataList.Find(x => x.Date.Equals(date));
            last7Days[label] = data?.Time ?? 0;
            sb_weekly.Append($"{label} : {last7Days[label]} \n");
        }

        sb_monthly.Clear();
        for (int i = 29; i >= 0; i--)
        {
            DateTime day = today.AddDays(-i);
            string label = day.ToString("MM/dd");
            string date = day.ToString("yyyy-MM-dd");
            
            DailyTimeItemData data = userDailyTimeData.DailyTimeItemDataList.Find(x => x.Date.Equals(date));
            last30Days[label] = data?.Time ?? 0;
            sb_monthly.Append($"{label} : {last30Days[label]} \n");
        }
        
        this.weeklyTotalTime.text = CalculateTimeFormat(weeklyTotalTime);
        weeklyTime.text = sb_weekly.ToString();
        monthlyTime.text = sb_monthly.ToString();
    }

    private void SetSubjectTime()
    {
        UserSubjectTimeData userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        if (userSubjectTimeData == null)
        {
            Logger.Log($"{GetType()}::UserData is null");
            return;
        }

        sb_subject.Clear();
        foreach (var subject in userSubjectTimeData.SubjectTimeItemDataList)
        {
            sb_subject.Append($"{subject.Name} : {CalculateTimeFormat(subject.Time)} \n");
        }
        //subjectTime.text = sb_subject.ToString();
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
}
