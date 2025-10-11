using System;
using System.Collections;
using System.Linq;
using System.Text;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Logger = Common.Logger;

/// <summary>
/// 대시보드 UI를 관리하는 클래스
/// 학습 시간, AI 조언, 차트 등을 보여줍니다
/// </summary>
public class DashboardTabUI : BaseUI
{
    [Header("텍스트 UI 요소들")] 
    [SerializeField] private Text aiText; // AI 조언을 보여주는 텍스트
    [SerializeField] private TMP_Text totalTimeText; // 총 학습 시간을 보여주는 텍스트
    [SerializeField] private TMP_Text weeklyTotalTime; // 주간 총 학습 시간을 보여주는 텍스트
    [SerializeField] private TMP_Text subjectTime; // 과목별 학습 시간을 보여주는 텍스트
    
    [Header("차트 컴포넌트들")] 
    [SerializeField] private CustomPieChart pieChart; // 파이차트 컴포넌트
    [SerializeField] private CustomBarChart barChart; // 막대차트 컴포넌트
    [SerializeField] private CustomLineChart lineChart; // 꺾은선차트 컴포넌트
    
    [Header("빈 데이터일 때 보여줄 텍스트들")] 
    [SerializeField] private GameObject aiEmptyText; // AI 조언이 없을 때 보여줄 텍스트
    [SerializeField] private GameObject pieChartEmptyText; // 파이차트 데이터가 없을 때 보여줄 텍스트
    [SerializeField] private GameObject barChartEmptyText; // 막대차트 데이터가 없을 때 보여줄 텍스트
    [SerializeField] private GameObject lineChartEmptyText; // 꺾은선차트 데이터가 없을 때 보여줄 텍스트

    [Header("차트 내용 컨테이너들")] 
    [SerializeField] private GameObject pieChartContent; // 파이차트 실제 내용
    [SerializeField] private GameObject barChartContent; // 막대차트 실제 내용
    [SerializeField] private GameObject lineChartContent; // 꺾은선차트 실제 내용

    [Header("에러 핸들링")]
    [SerializeField] private ErrorHandler errorHandler; // 에러 핸들러 컴포넌트
    
    // 텍스트를 만들 때 사용하는 StringBuilder (메모리 효율성을 위해)
    private readonly StringBuilder _sb = new StringBuilder(); // 일반 용도
    private readonly StringBuilder _sbSubject = new StringBuilder(); // 과목별 시간 표시용
    
    private ChartManager chartManager; // 차트 관리자
    
    /// <summary>
    /// UI가 열릴 때 호출되는 설정 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);

        InitializeChartComponents(); // 차트 컴포넌트 초기화 시도
        InitializeChartManager(); // 차트 매니저 초기화
        RefreshAllData(); // 모든 데이터를 새로고침
    }

    /// <summary>
    /// 차트 컴포넌트들을 자동으로 찾아서 연결
    /// </summary>
    // private bool InitializeChartComponents()
    private void InitializeChartComponents()
    {
        ValidateChartComponent(pieChart, "PieChart"); // 각 차트 컴포넌트가 연결되었는지 확인
        ValidateChartComponent(barChart, "BarChart"); // 연결되지 않았으면 로그를 남김
        ValidateChartComponent(lineChart, "LineChart"); // (자동 연결 시도는 하지 않음)
    }

    private void ValidateChartComponent(MonoBehaviour chart, string chartName) // 차트 컴포넌트가 연결되었는지 확인하는 함수
    {
        if (chart == null) // 컴포넌트가 연결되지 않았으면 오류 로그를 남김
        {
            Logger.LogError($"{GetType()}::{chartName} 컴포넌트가 연결되지 않았습니다");
        }
    }
    
    // 차트 매니저 초기화
    private void InitializeChartManager()
    {
        chartManager = new ChartManager(
            pieChart, pieChartContent, pieChartEmptyText,
            barChart, barChartContent, barChartEmptyText,
            lineChart, lineChartContent, lineChartEmptyText
        );
    }

    private void RefreshAllData()
    {
        try
        {
            errorHandler?.Hide(); // 에러 패널 숨기기
            
            RefreshTotalTime();
            RefreshWeeklyTime();
            RefreshSubjectTime();
            chartManager?.UpdateAllCharts();
            RefreshAIAdvice();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::RefreshAllData 오류: {e.Message}");
            errorHandler?.Show(ErrorType.DataError, RefreshAllData); // 에러 패널 표시 및 재시도 콜백 설정
        }
    }

    private void RefreshTotalTime()
    {
        if (totalTimeText == null)
        {
            return;
        }

        var userData = UserDataManager.Instance.GetUserData<UserTimeData>();
        if (userData == null)
        {
            errorHandler?.Show(ErrorType.DataError, RefreshAllData);
            return;
        }

        totalTimeText.text = FormatStudyTime(userData.Time);
    }

    private void RefreshWeeklyTime()
    {
        if (weeklyTotalTime == null)
        {
            return;
        }
        
        var dailyData = UserDataManager.Instance.GetUserData<UserDailyTimeData>();
        if (dailyData == null)
        {
            errorHandler?.Show(ErrorType.DataError, RefreshAllData);
            return;
        }

        long weeklyTotal = CalculateWeeklyTotal(dailyData);
        weeklyTotalTime.text = FormatStudyTime(weeklyTotal);
    }

    private void RefreshSubjectTime()
    {
        if (subjectTime == null) 
        {
            return;
        }
        
        var subjectData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        if (subjectData == null)
        {
            errorHandler?.Show(ErrorType.DataError, RefreshAllData);
            return;
        }

        _sbSubject.Clear();

        foreach (var item in subjectData.SubjectTimeItemDataList.OrderByDescending(x => x.Time))
        {
            _sbSubject.AppendLine($"{item.Name}: {FormatStudyTime(item.Time)}");
        }
        
        subjectTime.text = _sbSubject.ToString();
    }
    
    private void RefreshAIAdvice()
    {
        var adviceData = UserDataManager.Instance.GetUserData<UserLastAdviceData>();
        if (adviceData == null)
        {
            errorHandler?.Show(ErrorType.DataError, RefreshAllData);
            return;
        }
        
        string today = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");

        if (adviceData.Date == today && !string.IsNullOrEmpty(adviceData.Advice))
        {
            ShowAIState(adviceData.Advice);
        }
        else
        {
            ShowEmptyState(aiEmptyText, aiText.gameObject);
            RequestAIAdvice();
        }
    }
    
    private void RequestAIAdvice()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            errorHandler?.Show(ErrorType.NetworkError, RequestAIAdvice);
            return;
        }
        
        string studyContext = BuildStudyContext();
        StartCoroutine(RequestOpenAIAdvice(studyContext));
    }
    
    private IEnumerator RequestOpenAIAdvice(string context)
    {
        string apiKey = FirebaseManager.Instance.GetOpenAIKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.LogError($"{GetType()}::OpenAI API 키 없음");
            errorHandler?.Show(ErrorType.DataError);
            yield break;
        }

        var requestData = new OpenAIRequest
        {
            model = Constants.OpenAI.MODEL,
            messages = new OpenAIMessage[]
            {
                new OpenAIMessage { role = "system", content = "당신은 학습 코치입니다. 학생의 학습 패턴을 분석하고 간단한 조언을 제공하세요." },
                new OpenAIMessage { role = "user", content = context }
            },
            max_tokens = Constants.OpenAI.MAX_TOKENS,
        };
        
        string jsonData = JsonUtility.ToJson(requestData);

        using UnityWebRequest request = new UnityWebRequest(Constants.OpenAI.API_URL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        HandleAIResponse(request);
    }

    // OpenAI 응답 처리
    private void HandleAIResponse(UnityWebRequest request)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError($"{GetType()}::AI 요청 실패: {request.error}");
            
            if (errorHandler != null)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    errorHandler.Show(ErrorType.NetworkError, RequestAIAdvice);
                }
                else
                {
                    errorHandler.Show(ErrorType.DataError, RequestAIAdvice);
                }
            }
            return;
        }

        try
        {
            var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
            string advice = response.choices[0].message.content;

            var adviceData = UserDataManager.Instance.GetUserData<UserLastAdviceData>();
            string today = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");

            adviceData.Date = today;
            adviceData.Advice = advice;
            adviceData.SaveData();

            ShowAIState(advice);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::AI 응답 파싱 실패: {e.Message}");
            errorHandler?.Show(ErrorType.DataError, RequestAIAdvice);
        }
    }

    private string BuildStudyContext()
    {
        _sb.Clear();
        _sb.AppendLine("다음은 사용자의 최근 학습 데이터입니다:");
        
        var timeData = UserDataManager.Instance.GetUserData<UserTimeData>();
        if (timeData != null)
        {
            _sb.AppendLine($"- 총 학습 시간: {FormatStudyTime(timeData.Time)}");
        }
        
        var dailyData = UserDataManager.Instance.GetUserData<UserDailyTimeData>();
        if (dailyData != null)
        {
            long weeklyTotal = CalculateWeeklyTotal(dailyData);
            _sb.AppendLine($"- 최근 7일간 학습 시간: {FormatStudyTime(weeklyTotal)}");
        }
        
        var subjectData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        if (subjectData != null && subjectData.SubjectTimeItemDataList.Count > 0)
        {
            _sb.AppendLine("- 과목별 학습 시간:");
            foreach (var item in subjectData.SubjectTimeItemDataList.OrderByDescending(x => x.Time))
            {
                _sb.AppendLine($" {item.Name}: {FormatStudyTime(item.Time)}");
            }
        }
        
        _sb.AppendLine("이 데이터를 바탕으로 2~3문장으로 간단한 학습 조언을 제공해주세요.");
        return _sb.ToString();
    }
    
    private string FormatStudyTime(long totalSeconds)
    {
        int hours = (int)(totalSeconds / 3600);
        int minutes = (int)((totalSeconds % 3600) / 60);

        if (hours > 0)
        {
            return $"{hours}시간 {minutes}분";
        }
        else
        {
            return $"{minutes}분";
        }
    }

    private long CalculateWeeklyTotal(UserDailyTimeData dailyData)
    {
        DateTime now = DateTime.UtcNow.AddHours(9);
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

    private void ShowEmptyState(GameObject emptyText, GameObject content)
    {
        emptyText?.SetActive(true);
        content?.SetActive(false);
    }
    
    private void ShowAIState(string advice)
    {
        aiEmptyText?.SetActive(false);
        
        if (aiText != null)
        {
            aiText.gameObject.SetActive(true);
            aiText.text = advice;
        }
    }
    
    public void OnRefreshButtonClicked()
    {
        Logger.Log($"{GetType()}::데이터 새로고침 버튼 클릭됨");
        RefreshAllData();
    }

    [Serializable]
    private class OpenAIRequest
    {
        public string model;
        public OpenAIMessage[] messages;
        public int max_tokens;
    }
    
    [Serializable]
    private class OpenAIMessage
    {
        public string role;
        public string content;
    }
    
    [Serializable]
    private class OpenAIResponse
    {
        public Choice[] choices;
        
        [Serializable]
        public class Choice
        {
            public Message message;
        }
        
        [Serializable]
        public class Message
        {
            public string content;
        }
    }
}