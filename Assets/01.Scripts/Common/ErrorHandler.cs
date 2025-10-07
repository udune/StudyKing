using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    // 에러 타입 열거형
    public class ErrorHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject errorPanel; // 에러 메시지를 표시할 패널
        [SerializeField] private TextMeshProUGUI errorMessageText; // 에러 메시지 텍스트
        [SerializeField] private Button retryButton; // 재시도 버튼

        private Action onRetry; // 재시도 콜백

        private void Start() // 초기화
        {
            if (retryButton != null) // 널 체크
            {
                retryButton.onClick.AddListener(OnClickRetry); // 버튼 클릭 이벤트 등록
            }

            Hide(); // 시작 시 에러 패널 숨기기
        }

        public void Show(ErrorType errorType, Action retryAction = null) // 에러 패널 표시
        {
            onRetry = retryAction; // 재시도 콜백 설정
            errorMessageText.text = GetErrorMessage(errorType); // 에러 메시지 설정
            errorPanel.SetActive(true); // 에러 패널 표시

            if (retryButton != null) // 널 체크
            {
                retryButton.gameObject.SetActive(retryAction != null); // 재시도 콜백이 있으면 버튼 표시, 없으면 숨기기
            }
        }

        public void Hide() // 에러 패널 숨기기
        {   
            errorPanel.SetActive(false); // 에러 패널 숨기기
        }

        private void OnClickRetry()
        {
            Hide(); // 에러 패널 숨기기
            onRetry?.Invoke(); // 재시도 콜백 호출
        }

        private string GetErrorMessage(ErrorType errorType) // 에러 메시지 반환
        {
            switch (errorType) // 에러 타입에 따른 메시지 반환
            {
                case ErrorType.NetworkError:
                    return "네트워크 오류가 발생했습니다. 연결을 확인해주세요.";
                case ErrorType.DataError:
                    return "데이터 처리 중 오류가 발생했습니다. 다시 시도해주세요.";
                case ErrorType.ChartLoadError:
                    return "차트 로드 중 오류가 발생했습니다. 다시 시도해주세요.";
                case ErrorType.NoData:
                    return "표시할 데이터가 없습니다.";
                default:
                    return "알 수 없는 오류가 발생했습니다.";
            }
        }
    }
}