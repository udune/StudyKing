using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class DashboardTabUI : BaseUI
{
    [SerializeField] private TMP_Text totalTime;
    [SerializeField] private TMP_Text weeklyTotalTime;
    [SerializeField] private TMP_Text subjectTime;
    [SerializeField] private PieChart pieChart;
    [SerializeField] private BarChart barChart;
    [SerializeField] private GraphChart graphChart;
    
    Dictionary<string, long> last7Days = new Dictionary<string, long>();
    Dictionary<string, long> last30Days = new Dictionary<string, long>();
    
    private StringBuilder sb = new StringBuilder();
    private StringBuilder sb_subject = new StringBuilder();
    
    private ChartDynamicMaterial chartDynamicMaterial = new ChartDynamicMaterial();
    [SerializeField] private Material[] pieChartMaterials;
    [SerializeField] private Material barChartMaterial;
    
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
        DateTime today = DateTime.UtcNow.AddHours(9).Date;
        int difference = (int)today.DayOfWeek == 0 ? -6 : -(int)today.DayOfWeek - 1;
        DateTime monday = today.AddDays(difference).Date;
        foreach (var dailyTime in userDailyTimeData.DailyTimeItemDataList)
        {
            if (DateTime.TryParseExact(dailyTime.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
            {
                date = date.Date;
                if (date >= monday && date <= today)
                {
                    weeklyTotalTime += dailyTime.Time;   
                }
            }
        }
        this.weeklyTotalTime.text = CalculateTimeFormat(weeklyTotalTime);

        barChart.DataSource.ClearCategories();
        for (int i = 6; i >= 0; i--)
        {
            DateTime day = today.AddDays(-i);
            string label = day.ToString("ddd");
            string date = day.ToString("yyyy-MM-dd");
            
            DailyTimeItemData data = userDailyTimeData.DailyTimeItemDataList.Find(x => x.Date.Equals(date));
            last7Days[label] = data?.Time ?? 0;
            
            barChart.DataSource.AddCategory(label, chartDynamicMaterial);
            barChart.DataSource.SetValue(label, "weekly", last7Days[label]);
            barChart.DataSource.SetMaterial(label, barChartMaterial);
        }
        
        for (int i = 29; i >= 0; i--)
        {
            DateTime day = today.AddDays(-i);
            string label = day.ToString("MM/dd");
            string date = day.ToString("yyyy-MM-dd");
            
            DailyTimeItemData data = userDailyTimeData.DailyTimeItemDataList.Find(x => x.Date.Equals(date));
            last30Days[label] = data?.Time ?? 0;
            
            graphChart.DataSource.AddPointToCategory("monthly", 30-i, last30Days[label]);
        }
        
    }

    private void SetSubjectTime()
    {
        UserSubjectTimeData userSubjectTimeData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        if (userSubjectTimeData == null)
        {
            Logger.Log($"{GetType()}::UserData is null");
            return;
        }

        var topSubjects = userSubjectTimeData.SubjectTimeItemDataList
            .OrderByDescending(subject => subject.Time)
            .ToList();
        
        sb_subject.Clear();
        foreach (var subject in topSubjects)
        {
            sb_subject.Append($"{subject.Name} : {CalculateTimeFormat(subject.Time)} \n");
        }
        subjectTime.text = sb_subject.ToString();

        pieChart.DataSource.Clear();
        for (int i = 0; i < 3; i++)
        {
            pieChart.DataSource.AddCategory(topSubjects[i].Name, chartDynamicMaterial, 1, 1, 1);
            pieChart.DataSource.SetValue(topSubjects[i].Name, topSubjects[i].Time);
            pieChart.DataSource.SetMaterial(topSubjects[i].Name, pieChartMaterials[i]);
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
}
