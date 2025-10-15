using System;
using System.Collections;
using System.Text;
using Common;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Common.Logger;

namespace _01.Scripts.Manager
{
    // AI 조언 관리자 클래스
    public class AIAdviceManager
    {
        private readonly MonoBehaviour context; // MonoBehaviour 컨텍스트
        private readonly ErrorHandler errorHandler; // 에러 핸들러
        
        public AIAdviceManager(MonoBehaviour context, ErrorHandler errorHandler)
        {
            this.context = context;
            this.errorHandler = errorHandler;
        }
        
        // 새로운 조언 요청
        public void GetTodayAdvice(Action<string> onSuccess)
        {
            var adviceData = UserDataManager.Instance.GetUserData<UserLastAdviceData>();
            if (adviceData == null)
            {
                errorHandler?.Show(ErrorType.DataError); // 데이터 에러 처리
                return;
            }
        
            string today = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd"); // 한국 시간 기준 오늘 날짜

            if (adviceData.Date == today && !string.IsNullOrEmpty(adviceData.Advice)) // 오늘 날짜와 조언이 있으면
            {
                onSuccess?.Invoke(adviceData.Advice); // 기존 조언 반환
            }
            else
            {
                RequestNewAdvice(); // 새로운 조언 요청
            }
        }
        
        public void RequestNewAdvice(Action<string> onSuccess = null) // 새로운 조언 요청
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                errorHandler?.Show(ErrorType.NetworkError, () => RequestNewAdvice(onSuccess));
                return;
            }
        
            string studyContext = BuildStudyContext();
            context.StartCoroutine(SendAIRequest(studyContext, onSuccess));
        }
        
        private IEnumerator SendAIRequest(string studyContext, Action<string> onSuccess)
        {
            string apiKey = FirebaseManager.Instance.GetOpenAIKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                Logger.LogError("AIAdviceManager::OpenAI API 키 없음");
                errorHandler?.Show(ErrorType.DataError);
                yield break;
            }

            var requestData = new OpenAIRequest
            {
                model = Constants.OpenAI.MODEL,
                messages = new OpenAIMessage[]
                {
                    new OpenAIMessage { role = "system", content = "당신은 학습 코치입니다. 학생의 학습 패턴을 분석하고 간단한 조언을 제공하세요." },
                    new OpenAIMessage { role = "user", content = studyContext }
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

            HandleResponse(request, onSuccess);
        }
        
        private void HandleResponse(UnityWebRequest request, Action<string> onSuccess)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError($"AIAdviceManager::AI 요청 실패: {request.error}");
            
                if (errorHandler != null)
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        errorHandler.Show(ErrorType.NetworkError, () => RequestNewAdvice(onSuccess));
                    }
                    else
                    {
                        errorHandler.Show(ErrorType.DataError, () => RequestNewAdvice(onSuccess));
                    }
                }
                return;
            }

            try
            {
                var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                string advice = response.choices[0].message.content;

                SaveAdvice(advice);
                onSuccess?.Invoke(advice);
            }
            catch (Exception e)
            {
                Logger.LogError($"AIAdviceManager::AI 응답 파싱 실패: {e.Message}");
                errorHandler?.Show(ErrorType.DataError, () => RequestNewAdvice(onSuccess));
            }
        }
        
        private void SaveAdvice(string advice)
        {
            var adviceData = UserDataManager.Instance.GetUserData<UserLastAdviceData>();
            string today = DateTime.UtcNow.AddHours(9).Date.ToString("yyyy-MM-dd");

            adviceData.Date = today;
            adviceData.Advice = advice;
            adviceData.SaveData();
        }
        
        private string BuildStudyContext()
        {
            var timeData = UserDataManager.Instance.GetUserData<UserTimeData>();
            var subjectData = UserDataManager.Instance.GetUserData<UserSubjectTimeData>();
        
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"총 학습 시간: {FormatStudyTime(timeData?.Time ?? 0)}");
        
            if (subjectData != null && subjectData.SubjectTimeItemDataList.Count > 0)
            {
                sb.AppendLine("과목별 학습 시간:");
                foreach (var item in subjectData.SubjectTimeItemDataList)
                {
                    sb.AppendLine($"- {item.Name}: {FormatStudyTime(item.Time)}");
                }
            }
        
            return sb.ToString();
        }
        
        private string FormatStudyTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
        
            if (hours > 0 && minutes > 0)
            {
                return $"{hours}시간 {minutes}분";
            }
            else if (hours > 0)
            {
                return $"{hours}시간";
            }
            else if (minutes > 0)
            {
                return $"{minutes}분";
            }
            else
            {
                return "0분";
            }
        }
        
        // OpenAI API 요청/응답 클래스들
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
}