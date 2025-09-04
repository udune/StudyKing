using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Common.Logger;

/// <summary>
/// 대시보드 UI를 관리하는 클래스
/// 학습 시간, AI 조언, 차트 등을 보여줍니다
/// </summary>
public class DashboardTabUI : BaseUI
{
    [Header("텍스트 UI 요소들")]
    [SerializeField] private TMP_Text aiText;           // AI 조언을 보여주는 텍스트
    [SerializeField] private TMP_Text totalTimeText;        // 총 학습 시간을 보여주는 텍스트
    [SerializeField] private TMP_Text weeklyTotalTime;  // 주간 총 학습 시간을 보여주는 텍스트
    [SerializeField] private TMP_Text subjectTime;      // 과목별 학습 시간을 보여주는 텍스트
    
    [Header("차트 컨테이너들")]
    [SerializeField] private Transform pieChartContainer;   // 파이차트가 들어갈 부모 오브젝트
    [SerializeField] private Transform barChartContainer;   // 막대차트가 들어갈 부모 오브젝트
    [SerializeField] private Transform lineChartContainer;  // 꺾은선차트가 들어갈 부모 오브젝트
    
    [Header("빈 데이터일 때 보여줄 텍스트들")]
    [SerializeField] private GameObject aiEmptyText;           // AI 조언이 없을 때 보여줄 텍스트
    [SerializeField] private GameObject pieChartEmptyText;     // 파이차트 데이터가 없을 때 보여줄 텍스트
    [SerializeField] private GameObject barChartEmptyText;     // 막대차트 데이터가 없을 때 보여줄 텍스트
    [SerializeField] private GameObject lineChartEmptyText;    // 꺾은선차트 데이터가 없을 때 보여줄 텍스트
    
    [Header("차트 내용 컨테이너들")]
    [SerializeField] private GameObject pieChartContent;     // 파이차트 실제 내용
    [SerializeField] private GameObject barChartContent;     // 막대차트 실제 내용
    [SerializeField] private GameObject lineChartContent;    // 꺾은선차트 실제 내용
    
    // 텍스트를 만들 때 사용하는 StringBuilder (메모리 효율성을 위해)
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly StringBuilder _sbSubject = new StringBuilder();
    
    /// <summary>
    /// UI가 열릴 때 호출되는 설정 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);
        
        // 모든 차트와 텍스트 데이터를 새로고침합니다
        RefreshAllData();
    }
    
    /// <summary>
    /// 모든 데이터를 새로고침하는 함수
    /// </summary>
    private void RefreshAllData()
    {
        SetTotalTime();      // 총 학습 시간 설정
        SetWeeklyTime();     // 주간 학습 시간 설정
        SetSubjectTime();    // 과목별 학습 시간 설정
        SetAIAdvice();       // AI 조언 설정
        UpdateCharts();      // 차트 업데이트
    }
    
    /// <summary>
    /// AI 조언을 설정하는 함수
    /// 하루에 한 번만 새로운 조언을 받아옵니다
    /// </summary>
    private void SetAIAdvice()
    {
        if (aiText == null)
        {
            Logger.LogWarning($"{GetType()}::aiText가 연결되지 않았습니다");
            return;
        }

        try
        {
            // 사용자의 마지막 AI 조언 데이터를 가져옵니다
            UserLastAdviceData userLastAdviceData = UserDataManager.Instance?.GetUserData<UserLastAdviceData>();
            if (userLastAdviceData == null)
            {
                Logger.Log($"{GetType()}::UserLastAdviceData를 새로 생성합니다");
                userLastAdviceData = new UserLastAdviceData();
            }

            // 오늘 날짜를 문자열로 만듭니다
            string today = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");
            
            // 오늘 이미 조언을 받았으면 저장된 조언을 보여줍니다
            if (userLastAdviceData.Date == today && !string.IsNullOrEmpty(userLastAdviceData.Advice))
            {
                aiText.text = userLastAdviceData.Advice;
                if (aiEmptyText != null)
                    aiEmptyText.SetActive(false);
                return;
            }

            // 새로운 조언을 받아야 하는 경우
            StartCoroutine(RequestAIAdvice(userLastAdviceData));
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SetAIAdvice 오류: {e.Message}");
            ShowAIError();
        }
    }
    
    /// <summary>
    /// OpenAI API를 사용해서 AI 조언을 요청하는 코루틴
    /// </summary>
    private IEnumerator RequestAIAdvice(UserLastAdviceData userLastAdviceData)
    {
        // Firebase RemoteConfig에서 API 키 가져오기
        string apiKey = FirebaseManager.Instance?.GetOpenAIKey();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.LogError($"{GetType()}::OpenAI API 키를 가져올 수 없습니다");
            ShowAIError("API 키를 찾을 수 없습니다");
            yield break;
        }

        // 사용자의 학습 데이터를 바탕으로 AI에게 보낼 메시지 생성
        string promptMessage = CreatePromptMessage();
        
        // OpenAI API 요청 데이터 생성
        var requestData = new OpenAIRequest
        {
            messages = new List<Message>
            {
                new Message { role = "user", content = promptMessage }
            }
        };

        // JSON 변환
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);
        
        // UnityWebRequest 생성 및 설정
        UnityWebRequest request = null;
        
        try
        {
            request = new UnityWebRequest(Constants.OpenAI.API_URL, "POST");
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                
            // 로딩 표시
            if (aiText != null)
                aiText.text = "AI가 조언을 생각하고 있어요...";
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::HTTP 요청 생성 중 오류: {e.Message}");
            ShowAIError("요청 생성 중 오류가 발생했습니다");
            request?.Dispose();
            yield break;
        }
        
        yield return request.SendWebRequest();
        
        // 요청 결과 처리
        try
        {
            HandleAIResponse(request, userLastAdviceData);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::AI 응답 처리 중 오류: {e.Message}");
            ShowAIError("응답 처리 중 오류가 발생했습니다");
        }
        finally
        {
            // 리소스 정리
            request?.Dispose();
        }
    }
    
    /// <summary>
    /// AI에게 보낼 프롬프트 메시지를 만드는 함수
    /// </summary>
    private string CreatePromptMessage()
    {
        _sb.Clear();
        _sb.Append("사용자의 학습 데이터를 바탕으로 간단한 조언을 해주세요.\n");
        
        // 총 학습 시간 추가
        UserTimeData userTimeData = UserDataManager.Instance?.GetUserData<UserTimeData>();
        if (userTimeData != null && userTimeData.Time > 0)
        {
            _sb.Append($"총 학습 시간: {CalculateTimeFormat(userTimeData.Time)}\n");
        }
        else
        {
            _sb.Append("총 학습 시간: 아직 기록이 없음\n");
        }
        
        // 과목별 학습 시간 추가
        UserSubjectTimeData userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
        if (userSubjectTimeData != null && userSubjectTimeData.SubjectTimeItemDataList.Count > 0)
        {
            _sb.Append("과목별 학습 시간:\n");
            foreach (var subject in userSubjectTimeData.SubjectTimeItemDataList.Take(3))
            {
                _sb.Append($"- {subject.Name}: {CalculateTimeFormat(subject.Time)}\n");
            }
        }
        else
        {
            _sb.Append("과목별 데이터: 아직 기록이 없음\n");
        }
        
        _sb.Append("\n한국어로 50자 이내의 따뜻하고 격려가 되는 학습 조언을 해주세요.");
        return _sb.ToString();
    }
    
    /// <summary>
    /// AI 응답을 처리하는 함수
    /// </summary>
    private void HandleAIResponse(UnityWebRequest request, UserLastAdviceData userLastAdviceData)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            // 요청이 실패한 경우
            Logger.LogError($"{GetType()}::AI 요청 실패: {request.error}");
            ShowAIError($"네트워크 오류: {request.error}");
            return;
        }
        
        try
        {
            // 응답을 파싱합니다
            string responseText = request.downloadHandler.text;
            Logger.Log($"{GetType()}::OpenAI 응답 받음: {responseText}");
            
            var response = JsonUtility.FromJson<OpenAIResponse>(responseText);
            
            if (response?.choices == null || response.choices.Length == 0)
            {
                Logger.LogError($"{GetType()}::OpenAI 응답에 choices가 없습니다");
                ShowAIError("AI 응답 형식이 올바르지 않습니다");
                return;
            }
            
            string advice = response.choices[0].message.content?.Trim();
            
            if (string.IsNullOrEmpty(advice))
            {
                Logger.LogError($"{GetType()}::OpenAI 응답이 비어있습니다");
                ShowAIError("AI가 빈 응답을 보냈습니다");
                return;
            }
            
            // UI에 조언을 표시합니다
            if (aiText != null)
                aiText.text = advice;
            
            if (aiEmptyText != null)
                aiEmptyText.SetActive(false);
            
            // 조언을 저장합니다
            userLastAdviceData.Date = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");
            userLastAdviceData.Advice = advice;
            userLastAdviceData.SaveData();
            
            Logger.Log($"{GetType()}::AI 조언 받기 성공: {advice}");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::AI 응답 파싱 실패: {e.Message}");
            ShowAIError("AI 응답을 처리하는 중 오류가 발생했습니다");
        }
    }
    
    /// <summary>
    /// AI 오류 메시지를 표시하는 함수
    /// </summary>
    private void ShowAIError(string customMessage = null)
    {
        if (aiText != null)
        {
            aiText.text = customMessage ?? "AI 조언을 가져올 수 없습니다";
        }
        
        if (aiEmptyText != null)
        {
            aiEmptyText.SetActive(true);
        }
    }
    
    /// <summary>
    /// 총 학습 시간을 설정하는 함수
    /// </summary>
    private void SetTotalTime()
    {
        if (totalTimeText == null) return;

        try
        {
            UserTimeData userTimeData = UserDataManager.Instance?.GetUserData<UserTimeData>();
            if (userTimeData != null && userTimeData.Time > 0)
            {
                totalTimeText.text = CalculateTimeFormat(userTimeData.Time);
            }
            else
            {
                totalTimeText.text = "0시간";
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SetTotalTime 오류: {e.Message}");
            totalTimeText.text = "오류";
        }
    }
    
    /// <summary>
    /// 주간 총 학습 시간을 설정하는 함수
    /// </summary>
    private void SetWeeklyTime()
    {
        if (weeklyTotalTime == null) return;

        try
        {
            UserDailyTimeData userDailyTimeData = UserDataManager.Instance?.GetUserData<UserDailyTimeData>();
            if (userDailyTimeData?.DailyTimeItemDataList == null)
            {
                weeklyTotalTime.text = "0시간";
                return;
            }

            // 최근 7일간의 데이터를 합산합니다
            DateTime now = DateTime.UtcNow.AddHours(9);
            long weeklyTotal = 0;

            for (int i = 0; i < 7; i++)
            {
                string date = now.AddDays(-i).ToString("yyyy-MM-dd");
                var dayData = userDailyTimeData.DailyTimeItemDataList.FirstOrDefault(x => x.Date == date);
                if (dayData != null)
                {
                    weeklyTotal += dayData.Time;
                }
            }

            weeklyTotalTime.text = CalculateTimeFormat(weeklyTotal);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SetWeeklyTime 오류: {e.Message}");
            weeklyTotalTime.text = "오류";
        }
    }
    
    /// <summary>
    /// 과목별 학습 시간을 설정하는 함수
    /// </summary>
    private void SetSubjectTime()
    {
        if (subjectTime == null) return;

        try
        {
            UserSubjectTimeData userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
            if (userSubjectTimeData?.SubjectTimeItemDataList == null || 
                userSubjectTimeData.SubjectTimeItemDataList.Count == 0)
            {
                subjectTime.text = "과목별 데이터가 없습니다";
                return;
            }

            // 상위 3개 과목을 시간순으로 표시합니다
            _sbSubject.Clear();
            var topSubjects = userSubjectTimeData.SubjectTimeItemDataList
                .OrderByDescending(x => x.Time)
                .Take(3);

            foreach (var subject in topSubjects)
            {
                if (_sbSubject.Length > 0)
                    _sbSubject.AppendLine();
                
                _sbSubject.Append($"{subject.Name}: {CalculateTimeFormat(subject.Time)}");
            }

            subjectTime.text = _sbSubject.ToString();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SetSubjectTime 오류: {e.Message}");
            subjectTime.text = "오류";
        }
    }
    
    /// <summary>
    /// 차트들을 업데이트하는 함수
    /// </summary>
    private void UpdateCharts()
    {
        UpdatePieChart();
        UpdateBarChart();
        UpdateLineChart();
    }
    
    /// <summary>
    /// 파이차트를 업데이트하는 함수
    /// </summary>
    private void UpdatePieChart()
    {
        try
        {
            UserSubjectTimeData userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
            bool hasData = userSubjectTimeData?.SubjectTimeItemDataList?.Count > 0;

            if (pieChartContent != null)
                pieChartContent.SetActive(hasData);
            
            if (pieChartEmptyText != null)
                pieChartEmptyText.SetActive(!hasData);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UpdatePieChart 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 막대차트를 업데이트하는 함수
    /// </summary>
    private void UpdateBarChart()
    {
        try
        {
            UserDailyTimeData userDailyTimeData = UserDataManager.Instance?.GetUserData<UserDailyTimeData>();
            bool hasData = userDailyTimeData?.DailyTimeItemDataList?.Count > 0;

            if (barChartContent != null)
                barChartContent.SetActive(hasData);
            
            if (barChartEmptyText != null)
                barChartEmptyText.SetActive(!hasData);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UpdateBarChart 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 꺾은선 차트를 업데이트하는 함수
    /// </summary>
    private void UpdateLineChart()
    {
        try
        {
            UserHistoryData userHistoryData = UserDataManager.Instance?.GetUserData<UserHistoryData>();
            bool hasData = userHistoryData?.HistoryItemDataList?.Count > 0;

            if (lineChartContent != null)
                lineChartContent.SetActive(hasData);
            
            if (lineChartEmptyText != null)
                lineChartEmptyText.SetActive(!hasData);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UpdateLineChart 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 시간을 포맷팅하는 함수 (초 -> "X시간 Y분" 형태)
    /// </summary>
    private string CalculateTimeFormat(long timeInSeconds)
    {
        if (timeInSeconds <= 0) return "0초";

        long hours = timeInSeconds / 3600;
        long minutes = (timeInSeconds % 3600) / 60;
        long seconds = timeInSeconds % 60;

        _sb.Clear();

        if (hours > 0)
        {
            _sb.Append($"{hours}시간");
            if (minutes > 0)
                _sb.Append($" {minutes}분");
        }
        else if (minutes > 0)
        {
            _sb.Append($"{minutes}분");
            if (seconds > 0)
                _sb.Append($" {seconds}초");
        }
        else
        {
            _sb.Append($"{seconds}초");
        }

        return _sb.ToString();
    }
    
    /// <summary>
    /// 수동으로 데이터를 새로고침하는 함수 (버튼에서 호출 가능)
    /// </summary>
    public void OnRefreshButtonClicked()
    {
        Logger.Log($"{GetType()}::수동 새로고침 버튼 클릭됨");
        RefreshAllData();
    }
}

#region OpenAI API Response Classes

/// <summary>
/// OpenAI API 응답을 받기 위한 클래스
/// </summary>
[Serializable]
public class OpenAIResponse
{
    public Choice[] choices; // AI의 응답 선택지들
}

/// <summary>
/// OpenAI API 응답의 선택지 클래스
/// </summary>
[Serializable]
public class Choice
{
    public Message message; // 메시지 내용
}

/// <summary>
/// OpenAI API의 메시지 클래스
/// </summary>
[Serializable]
public class Message
{
    public string role;    // 역할 (user, assistant)
    public string content; // 메시지 내용
}

/// <summary>
/// OpenAI API 요청을 위한 클래스
/// </summary>
[Serializable]
public class OpenAIRequest
{
    public string model = Constants.OpenAI.MODEL;           // 사용할 AI 모델
    public List<Message> messages;                          // 보낼 메시지들
    public int maxTokens = Constants.OpenAI.MAX_TOKENS;    // 최대 토큰 수
    public float temperature = Constants.OpenAI.TEMPERATURE; // AI 응답의 창의성 정도
}

#endregion