using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChartAndGraph;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Common.Logger;

[Serializable]
public class OpenAIResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class OpenAIRequest
{
    public string model = Constants.OpenAI.MODEL;
    public List<Message> messages;
    public int max_tokens = Constants.OpenAI.MAX_TOKENS;
    public float temperature = Constants.OpenAI.TEMPERATURE;
}

public class DashboardTabUI : BaseUI
{
    [SerializeField] private TMP_Text aiText;
    [SerializeField] private TMP_Text totalTime;
    [SerializeField] private TMP_Text weeklyTotalTime;
    [SerializeField] private TMP_Text subjectTime;
    [SerializeField] private PieChart pieChart;
    [SerializeField] private BarChart barChart;
    [SerializeField] private GraphChart graphChart;
    
    [SerializeField] private GameObject aiEmptyText;
    [SerializeField] private GameObject pieChartEmptyText;
    [SerializeField] private GameObject barChartEmptyText;
    [SerializeField] private GameObject graphChartEmptyText;
    
    [SerializeField] private GameObject pieChartContent;
    [SerializeField] private GameObject barChartContent;
    [SerializeField] private GameObject graphChartContent;
    
    Dictionary<string, long> last7Days = new Dictionary<string, long>()
    {
        { "월", 0 },
        { "화", 0 },
        { "수", 0 },
        { "목", 0 },
        { "금", 0 },
        { "토", 0 },
        { "일", 0 },
    };
    Dictionary<string, long> last30Days = new Dictionary<string, long>();
    
    private StringBuilder sb = new StringBuilder();
    private StringBuilder sb_subject = new StringBuilder();
    
    private ChartDynamicMaterial chartDynamicMaterial = new ChartDynamicMaterial();
    [SerializeField] private Material[] pieChartMaterials;
    [SerializeField] private Material barChartMaterial;
    
    private const string OPENAI_URL = Constants.OpenAI.API_URL;
    
    private readonly Dictionary<DayOfWeek, string> DayOfWeekKor = new Dictionary<DayOfWeek, string>
    {
        { DayOfWeek.Monday, "월" },
        { DayOfWeek.Tuesday, "화" },
        { DayOfWeek.Wednesday, "수" },
        { DayOfWeek.Thursday, "목" },
        { DayOfWeek.Friday, "금" },
        { DayOfWeek.Saturday, "토" },
        { DayOfWeek.Sunday, "일" },
    };
    
    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
#if !UNITY_EDITOR
        SetTotalTime();
        SetWeeklyTime();
        SetSubjectTime();
        
        RequestStudyAdvice();
#endif
    }

    private void RequestStudyAdvice()
    {
        UserLastAdviceData userLastAdviceData = UserDataManager.Instance.GetUserData<UserLastAdviceData>();
        if (userLastAdviceData == null)
        {
            Logger.Log($"{GetType()}::UserLastAdviceData is null");
            aiEmptyText.SetActive(true);
            return;
        }
        
        aiEmptyText.SetActive(false);

        if (userLastAdviceData.Date.Equals(DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd")))
        {
            aiText.text = userLastAdviceData.Advice;
            return;
        }

        string message = "다음은 한 사용자의 공부 기록입니다" +
                         $"- 총 공부 시간: {totalTime.text}" +
                         $"- 이번 주 공부 시간: {weeklyTotalTime.text}" +
                         $"- 과목별 공부 시간: {subjectTime.text}" +
                         "- 최근 7일간 요일별 공부 시간: " +
                         $"- 월: {last7Days["월"]}, 화: {last7Days["화"]}, 수: {last7Days["수"]}, 목: {last7Days["목"]}, 금: {last7Days["금"]}, 토: {last7Days["토"]}, 일: {last7Days["일"]}" +
                         "이 데이터를 바탕으로 사용자가 앞으로 어떤 방식으로 공부를 하면 좋을지 조언해 주세요. " +
                         "80자 이내로 한국어로 간단하게 응원 및 조언 메시지로 작성해 주세요. 이모지 없이 텍스트를 생성해주세요.";
        
        StartCoroutine(RequestOpenAI(message, userLastAdviceData));
    }

    private IEnumerator RequestOpenAI(string message, UserLastAdviceData userLastAdviceData)
    {
        var requestData = new OpenAIRequest()
        {
            messages = new List<Message>
            {
                new Message { role = "user", content = message }
            }
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        var uwr = new UnityWebRequest(OPENAI_URL, "POST");
        byte[] raw = Encoding.UTF8.GetBytes(jsonData);
        uwr.uploadHandler = new UploadHandlerRaw(raw);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Authorization", $"Bearer {FirebaseManager.Instance.GetOpenAIKey()}");
        
        yield return uwr.SendWebRequest();

        if (uwr.error != null)
        {
            aiText.text = "오늘의 학습 방향 추천을 불러오지 못했어요.";
            Logger.Log($"{GetType()}::OpenAI request failed: {uwr.result} {uwr.error}");
        }
        else if (uwr.result.Equals(UnityWebRequest.Result.Success))
        {
            Logger.Log($"{GetType()}::OpenAI request succeeded");
            
            var response = JsonUtility.FromJson<OpenAIResponse>(uwr.downloadHandler.text);
            string advice = response.choices[0].message.content;
            Logger.Log($"{GetType()}::OpenAI request response: {advice}");
            
            aiText.text = advice;
            
            userLastAdviceData.Date = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");
            userLastAdviceData.Advice = advice;
            userLastAdviceData.SaveData();
        }
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
            barChartEmptyText.SetActive(true);
            graphChartEmptyText.SetActive(true);
            barChartContent.SetActive(false);
            graphChartContent.SetActive(false);
            return;
        }

        if (userDailyTimeData.DailyTimeItemDataList.Count.Equals(0))
        {
            barChartEmptyText.SetActive(true);
            graphChartEmptyText.SetActive(true);
            barChartContent.SetActive(false);
            graphChartContent.SetActive(false);
            return;
        }
        
        barChartEmptyText.SetActive(false);
        graphChartEmptyText.SetActive(false);
        barChartContent.SetActive(true);
        graphChartContent.SetActive(true);
        
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
            string korDay = DayOfWeekKor[day.DayOfWeek];
            string date = day.ToString("yyyy-MM-dd");
            
            DailyTimeItemData data = userDailyTimeData.DailyTimeItemDataList.Find(x => x.Date.Equals(date));
            last7Days[korDay] = data?.Time ?? 0;
            
            barChart.DataSource.AddCategory(korDay, chartDynamicMaterial);
            barChart.DataSource.SetValue(korDay, "weekly", last7Days[korDay]);
            barChart.DataSource.SetMaterial(korDay, barChartMaterial);
        }
        
        for (int i = 29; i >= 0; i--)
        {
            var day = today.AddDays(-i);
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
            pieChartEmptyText.SetActive(true);
            pieChartContent.SetActive(false);
            return;
        }

        if (userSubjectTimeData.SubjectTimeItemDataList.Count.Equals(0))
        {
            pieChartEmptyText.SetActive(true);
            pieChartContent.SetActive(false);
            return;
        }
        
        pieChartEmptyText.SetActive(false);
        pieChartContent.SetActive(true);

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
        int count = Mathf.Min(3, topSubjects.Count);
        for (int i = 0; i < count; i++)
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
