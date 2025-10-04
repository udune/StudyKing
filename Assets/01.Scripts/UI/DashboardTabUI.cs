using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    [Header("차트 컨테이너들")] 
    [SerializeField] private Transform pieChartContainer; // 파이차트가 들어갈 부모 오브젝트
    [SerializeField] private Transform barChartContainer; // 막대차트가 들어갈 부모 오브젝트
    [SerializeField] private Transform lineChartContainer; // 꺾은선차트가 들어갈 부모 오브젝트

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

    // 텍스트를 만들 때 사용하는 StringBuilder (메모리 효율성을 위해)
    private readonly StringBuilder _sb = new StringBuilder(); // 일반 용도
    private readonly StringBuilder _sbSubject = new StringBuilder(); // 과목별 시간 표시용

    /// <summary>
    /// UI가 열릴 때 호출되는 설정 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);

        if (!InitializeChartComponents()) // 차트 컴포넌트 초기화에 실패하면 로그를 남기고 종료
        {
            Logger.LogError($"{GetType()}::차트 컴포넌트 초기화에 실패했습니다");
            return;
        }

        // 모든 차트와 텍스트 데이터를 새로고침합니다
        RefreshAllData();
    }

    /// <summary>
    /// 차트 컴포넌트들을 자동으로 찾아서 연결
    /// </summary>
    private bool InitializeChartComponents()
    {
        bool isValid = true; // 모든 컴포넌트가 유효한지 여부

        if (pieChartContainer == null) // 파이차트 컨테이너가 연결되지 않았으면 오류 로그를 남기고 false 반환
        {
            Logger.LogError($"{GetType()}::pieChartContainer가 연결되지 않았습니다");
            isValid = false;
        }
        else if (pieChart == null) // 파이차트 컴포넌트가 연결되지 않았으면 컨테이너에서 찾아서 연결 시도
        {
            pieChart = pieChartContainer.GetComponentInChildren<CustomPieChart>(); // 자식 오브젝트에서 CustomPieChart 컴포넌트를 찾음
            if (pieChart == null) // 그래도 못 찾으면 오류 로그를 남기고 false 반환
            {
                Logger.LogError($"{GetType()}::pieChart 컴포넌트를 찾을 수 없습니다");
                isValid = false;
            }
        }

        if (barChartContainer == null) // 막대차트 컨테이너가 연결되지 않았으면 오류 로그를 남기고 false 반환
        {
            Logger.LogError($"{GetType()}::barChartContainer가 연결되지 않았습니다");
            isValid = false;
        }
        else if (barChart == null) // 막대차트 컴포넌트가 연결되지 않았으면 컨테이너에서 찾아서 연결 시도
        {
            barChart = barChartContainer.GetComponentInChildren<CustomBarChart>(); // 자식 오브젝트에서 CustomBarChart 컴포넌트를 찾음
            if (barChart == null) // 그래도 못 찾으면 오류 로그를 남기고 false 반환
            {
                Logger.LogError($"{GetType()}::barChart 컴포넌트를 찾을 수 없습니다");
                isValid = false;
            }
        }

        if (lineChartContainer == null) // 꺾은선차트 컨테이너가 연결되지 않았으면 오류 로그를 남기고 false 반환
        {
            Logger.LogError($"{GetType()}::lineChartContainer가 연결되지 않았습니다");
            isValid = false;
        }
        else if (lineChart == null) // 꺾은선차트 컴포넌트가 연결되지 않았으면 컨테이너에서 찾아서 연결 시도
        {
            lineChart = lineChartContainer.GetComponentInChildren<CustomLineChart>(); // 자식 오브젝트에서 CustomLineChart 컴포넌트를 찾음
            if (lineChart == null) // 그래도 못 찾으면 오류 로그를 남기고 false 반환
            {
                Logger.LogError($"{GetType()}::lineChart 컴포넌트를 찾을 수 없습니다");
                isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// 모든 데이터를 새로고침하는 함수
    /// </summary>
    private void RefreshAllData()
    {
        UpdateTotalTime(); // 총 학습 시간 업데이트
        UpdateWeeklyTime(); // 주간 학습 시간 업데이트
        UpdateSubjectTime(); // 과목별 학습 시간 업데이트
        UpdatePieChart(); // 파이차트 업데이트
        UpdateBarChart(); // 막대차트 업데이트
        UpdateLineChart(); // 꺾은선차트 업데이트
        UpdateAIAdvice(); // AI 조언 업데이트
    }

    private void UpdateTotalTime() // 총 학습 시간 업데이트
    {
        if (totalTimeText == null) // 텍스트 컴포넌트가 연결되지 않았으면 종료
        {
            return;
        }

        UserTimeData timeData = UserDataManager.Instance?.GetUserData<UserTimeData>(); // 사용자 총 학습 시간 데이터 가져오기

        if (timeData != null && timeData.Time > 0) // 데이터가 유효하면 시간과 분으로 변환하여 표시
        {
            int hours = (int)(timeData.Time / 3600); // 초를 시간으로 변환
            int minutes = (int)((timeData.Time % 3600) / 60); // 남은 초를 분으로 변환
            totalTimeText.text = $"{hours}시간 {minutes}분"; // 텍스트에 표시
        }
        else
        {
            totalTimeText.text = "0시간"; // 데이터가 없으면 0시간으로 표시
        }
    }

    private void UpdateWeeklyTime() // 주간 학습 시간 업데이트
    {
        if (weeklyTotalTime == null) // 텍스트 컴포넌트가 연결되지 않았으면 종료
        {
            return;
        }

        UserDailyTimeData dailyData = UserDataManager.Instance?.GetUserData<UserDailyTimeData>(); // 사용자 일일 학습 시간 데이터 가져오기

        if (dailyData?.DailyTimeItemDataList == null) // 데이터가 없으면 0시간으로 표시하고 종료
        {
            weeklyTotalTime.text = "0시간";
            return;
        }

        DateTime now = DateTime.UtcNow.AddHours(9); // 한국 시간 기준 현재 날짜
        long weeklyTotal = 0; // 주간 총 학습 시간 (초 단위)

        for (int i = 0; i < 7; i++) // 최근 7일간의 데이터를 합산
        {
            string date = now.AddDays(-i).ToString("yyyy-MM-dd"); // 날짜 문자열 생성
            var dayData = dailyData.DailyTimeItemDataList.FirstOrDefault(x => x.Date == date); // 해당 날짜의 데이터 찾기
            if (dayData != null) // 데이터가 있으면 시간을 합산
            {
                weeklyTotal += dayData.Time; // 초 단위로 합산
            }
        }

        if (weeklyTotal > 0) // 합산된 시간이 있으면 시간과 분으로 변환하여 표시
        {
            int hours = (int)(weeklyTotal / 3600); // 초를 시간으로 변환
            int minutes = (int)((weeklyTotal % 3600) / 60); // 남은 초를 분으로 변환
            weeklyTotalTime.text = $"{hours}시간 {minutes}분"; // 텍스트에 표시
        }
        else
        {
            weeklyTotalTime.text = "0시간";
        }
    }

    private void UpdateSubjectTime() // 과목별 학습 시간 업데이트
    {
        if (subjectTime == null) // 텍스트 컴포넌트가 연결되지 않았으면 종료
        {
            return;
        }

        UserSubjectTimeData subjectData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>(); // 사용자 과목별 학습 시간 데이터 가져오기

        if (subjectData?.SubjectTimeItemDataList != null && subjectData.SubjectTimeItemDataList.Count > 0) // 데이터가 유효하면 각 과목별 시간을 계산하여 표시
        {
            _sbSubject.Clear(); // StringBuilder 초기화

            foreach (var subject in subjectData.SubjectTimeItemDataList) // 각 과목별로 반복
            {
                if (subject.Time > 0) // 학습 시간이 0보다 크면 시간과 분으로 변환하여 추가
                {
                    int hours = (int)(subject.Time / 3600); // 초를 시간으로 변환
                    int minutes = (int)((subject.Time % 3600) / 60); // 남은 초를 분으로 변환

                    if (hours > 0) // 시간이 있으면 시간과 분 모두 표시
                    {
                        _sbSubject.AppendLine($"{subject.Name}: {hours}시간 {minutes}분"); // 과목명: X시간 Y분
                    }
                    else
                    {
                        _sbSubject.AppendLine($"{subject.Name}: {minutes}분"); // 과목명: Y분
                    }
                }
            }

            subjectTime.text = _sbSubject.ToString().TrimEnd(); // 최종 문자열을 텍스트에 설정 (끝의 개행 문자 제거)
        }
        else
        {
            subjectTime.text = "데이터 없음"; // 데이터가 없으면 "데이터 없음"으로 표시
        }
    }

    private void UpdatePieChart() // 파이차트 업데이트
    {
        if (pieChart == null) // 파이차트 컴포넌트가 연결되지 않았으면 종료
        {
            Logger.LogWarning($"{GetType()}::pieChart 컴포넌트가 연결되지 않았습니다");
            return;
        }

        UserSubjectTimeData subjectData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>(); // 사용자 과목별 학습 시간 데이터 가져오기
        bool hasData = subjectData?.SubjectTimeItemDataList != null &&
                       subjectData.SubjectTimeItemDataList.Any(s => s.Time > 0); // 유효한 데이터가 있는지 확인

        if (pieChartContent != null) // 파이차트 내용 컨테이너가 있으면 데이터 유무에 따라 활성화/비활성화
        {
            pieChartContent.SetActive(hasData); // 데이터가 있으면 활성화
        }

        if (pieChartEmptyText != null) // 빈 데이터 텍스트가 있으면 데이터 유무에 따라 활성화/비활성화
        {
            pieChartEmptyText.SetActive(!hasData); // 데이터가 없으면 활성화
        }

        pieChart.ClearData(); // 기존 데이터를 모두 지움

        if (hasData) // 유효한 데이터가 있으면 파이차트에 데이터 추가
        {
            foreach (var subject in subjectData.SubjectTimeItemDataList) // 각 과목별로 반복
            {
                if (subject.Time > 0) // 학습 시간이 0보다 크면 파이차트에 추가
                {
                    float hours = subject.Time / 3600f; // 초를 시간으로 변환 (소수점 포함)
                    pieChart.AddData(subject.Name, hours); // 과목명과 시간을 파이차트에 추가
                }
            }

            pieChart.RefreshChart(); // 차트 갱신
        }
    }

    private void UpdateBarChart() // 막대차트 업데이트
    {
        try
        {
            UserSubjectTimeData userSubjectTimeData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
            bool hasData = userSubjectTimeData?.SubjectTimeItemDataList?.Count > 0;

            Logger.Log($"{GetType()}::막대차트 데이터 확인 - UserSubjectTimeData: {(userSubjectTimeData != null ? "있음" : "null")}, SubjectTimeItemDataList: {userSubjectTimeData?.SubjectTimeItemDataList?.Count ?? 0}개, hasData: {hasData}");

            if (barChartContent != null)
            {
                barChartContent.SetActive(hasData);
                Logger.Log($"{GetType()}::BarChartContent 활성화: {hasData}");
            }
        
            if (barChartEmptyText != null)
            {
                barChartEmptyText.SetActive(!hasData);
                Logger.Log($"{GetType()}::BarChartEmptyText 활성화: {!hasData}");
            }

            // 실제 막대차트 데이터 업데이트 (과목별 학습 시간)
            if (barChart == null)
            {
                Logger.LogWarning($"{GetType()}::BarChart 컴포넌트가 연결되지 않았습니다");
            }
            else if (hasData)
            {
                Logger.Log($"{GetType()}::막대차트 데이터 업데이트 시작 - {userSubjectTimeData.SubjectTimeItemDataList.Count}개 과목");
            
                barChart.ClearData();
            
                // 시간이 있는 과목들만 추가 (시간 순으로 정렬)
                var sortedSubjects = userSubjectTimeData.SubjectTimeItemDataList
                    .Where(x => x.Time > 0)
                    .OrderByDescending(x => x.Time)
                    .ToList();
            
                foreach (var subject in sortedSubjects)
                {
                    // 시간을 시간 단위로 변환 (초 -> 시간)
                    float hours = subject.Time / 3600f;
                    barChart.AddData(subject.Name, hours);
                    Logger.Log($"{GetType()}::막대차트 데이터 추가 - {subject.Name}: {hours:F2}시간");
                }
            
                barChart.RefreshChart();
                Logger.Log($"{GetType()}::막대차트 RefreshChart 호출 완료");
            }
            else
            {
                Logger.Log($"{GetType()}::막대차트 데이터 없음 - 차트 클리어");
                barChart.ClearData();
            }
        } catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UpdateBarChart 오류: {e.Message}");
        }
    }
    
    private void UpdateLineChart() // 꺾은선차트 업데이트
    {
        if (lineChart == null) // 꺾은선차트 컴포넌트가 연결되지 않았으면 종료
        {
            Logger.LogWarning($"{GetType()}::lineChart 컴포넌트가 연결되지 않았습니다");
            return;
        }

        UserDailyTimeData dailyData = UserDataManager.Instance?.GetUserData<UserDailyTimeData>(); // 사용자 일일 학습 시간 데이터 가져오기
        bool hasData = dailyData?.DailyTimeItemDataList != null &&
                       dailyData.DailyTimeItemDataList.Any(d => d.Time > 0); // 유효한 데이터가 있는지 확인

        if (lineChartContent != null) // 꺾은선차트 내용 컨테이너가 있으면 데이터 유무에 따라 활성화/비활성화
        {
            lineChartContent.SetActive(hasData); // 데이터가 있으면 활성화
        }

        if (lineChartEmptyText != null) // 빈 데이터 텍스트가 있으면 데이터 유무에 따라 활성화/비활성화
        {
            lineChartEmptyText.SetActive(!hasData); // 데이터가 없으면 활성화
        }

        lineChart.ClearData(); // 기존 데이터를 모두 지움

        if (hasData) // 유효한 데이터가 있으면 꺾은선차트에 데이터 추가
        {
            DateTime now = DateTime.UtcNow.AddHours(9); // 한국 시간 기준 현재 날짜

            for (int i = 6; i >= 0; i--) // 최근 7일간의 데이터를 날짜 순서대로 추가
            {
                string date = now.AddDays(-i).ToString("yyyy-MM-dd"); // 날짜 문자열 생성
                var dayData = dailyData.DailyTimeItemDataList.FirstOrDefault(x => x.Date == date); // 해당 날짜의 데이터 찾기

                float hours = 0; // 기본값은 0시간
                if (dayData != null && dayData.Time > 0) // 데이터가 있으면 시간을 시간 단위로 변환
                {
                    hours = dayData.Time / 3600f; // 초를 시간으로 변환 (소수점 포함)
                }
                
                string label = now.AddDays(-i).ToString("yyyy-MM-dd"); // 라벨은 날짜 문자열로 설정
                lineChart.AddData(label, hours); // 날짜와 시간을 꺾은선차트에 추가
            }
            
            lineChart.RefreshChart(); // 차트 갱신
        }
    }
    
    private void UpdateAIAdvice()
    {
        if (aiText == null) return;

        UserLastAdviceData adviceData = UserDataManager.Instance?.GetUserData<UserLastAdviceData>();
        if (adviceData == null)
        {
            adviceData = new UserLastAdviceData();
        }

        string today = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");

        // 오늘 이미 조언을 받았으면 표시
        if (adviceData.Date == today && !string.IsNullOrEmpty(adviceData.Advice))
        {
            aiText.text = adviceData.Advice;
            if (aiEmptyText != null)
                aiEmptyText.SetActive(false);
            return;
        }

        // 새 조언 요청
        StartCoroutine(RequestAIAdvice(adviceData));
    }

    private IEnumerator RequestAIAdvice(UserLastAdviceData adviceData)
    {
        string apiKey = FirebaseManager.Instance?.GetOpenAIKey();

        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.LogError($"{GetType()}::OpenAI API 키 없음");
            ShowAIError("API 키를 찾을 수 없습니다");
            yield break;
        }

        string promptMessage = CreatePromptMessage();

        var requestData = new OpenAIRequest
        {
            messages = new List<Message>
            {
                new Message { role = "user", content = promptMessage }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(Constants.OpenAI.API_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        if (aiText != null)
            aiText.text = "AI가 조언을 생각하고 있어요...";

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError($"{GetType()}::AI 요청 실패: {request.error}");
            ShowAIError($"요청 실패 (코드: {request.responseCode})");
            request.Dispose();
            yield break;
        }

        try
        {
            string responseText = request.downloadHandler.text;
            var response = JsonUtility.FromJson<OpenAIResponse>(responseText);

            if (response?.choices == null || response.choices.Length == 0)
            {
                ShowAIError("AI 응답 형식 오류");
                yield break;
            }

            string advice = response.choices[0].message.content?.Trim();

            if (string.IsNullOrEmpty(advice))
            {
                ShowAIError("AI가 빈 응답을 보냈습니다");
                yield break;
            }

            if (aiText != null)
                aiText.text = advice;

            if (aiEmptyText != null)
                aiEmptyText.SetActive(false);

            adviceData.Date = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");
            adviceData.Advice = advice;
            adviceData.SaveData();

            Logger.Log($"{GetType()}::AI 조언 성공: {advice}");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::AI 응답 파싱 실패: {e.Message}");
            ShowAIError("응답 처리 오류");
        }
        finally
        {
            request.Dispose();
        }
    }

    private string CreatePromptMessage()
    {
        _sb.Clear();
        _sb.Append("사용자의 학습 데이터를 바탕으로 간단한 조언을 해주세요.\n");

        UserTimeData timeData = UserDataManager.Instance?.GetUserData<UserTimeData>();
        if (timeData != null && timeData.Time > 0)
        {
            _sb.Append($"총 학습 시간: {timeData.Time / 3600}시간\n");
        }
        else
        {
            _sb.Append("총 학습 시간: 아직 기록이 없음\n");
        }

        UserSubjectTimeData subjectData = UserDataManager.Instance?.GetUserData<UserSubjectTimeData>();
        if (subjectData != null && subjectData.SubjectTimeItemDataList.Count > 0)
        {
            _sb.Append("과목별 학습 시간:\n");
            foreach (var subject in subjectData.SubjectTimeItemDataList.Take(3))
            {
                _sb.Append($"- {subject.Name}: {subject.Time / 3600}시간\n");
            }
        }

        _sb.Append("\n한국어로 50자 이내의 따뜻하고 격려가 되는 학습 조언을 해주세요.");
        return _sb.ToString();
    }
    
    private void ShowAIError(string message = null)
    {
        if (aiText != null)
            aiText.text = message ?? "AI 조언을 가져올 수 없습니다";

        if (aiEmptyText != null)
            aiEmptyText.SetActive(true);
    }

    public void Refresh() // 외부에서 호출하여 모든 데이터를 새로고침하는 공개 함수
    {
        RefreshAllData(); // 외부에서 호출하여 모든 데이터를 새로고침
    }
    
    #region OpenAI API Classes

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

    #endregion
}