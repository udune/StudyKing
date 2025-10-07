using Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Logger = Common.Logger;

/// <summary>
/// 계정 로그인 UI 클래스
/// 구글, 애플 등의 소셜 로그인을 처리합니다
/// </summary>
public class AccountUI : BaseUI
{
    [Header("로그인 버튼들")]
    [SerializeField] private Button googleLoginButton;    // 구글 로그인 버튼
    [SerializeField] private Button appleLoginButton;     // 애플 로그인 버튼
    [SerializeField] private Button guestLoginButton;     // 게스트 로그인 버튼
    
    [Header("UI 텍스트들")]
    [SerializeField] private TMP_Text titleText;          // 제목 텍스트
    [SerializeField] private TMP_Text descriptionText;    // 설명 텍스트
    [SerializeField] private TMP_Text statusText;         // 상태 표시 텍스트
    
    [Header("로딩 UI")]
    [SerializeField] private GameObject loadingPanel;     // 로딩 패널
    [SerializeField] private TMP_Text loadingText;        // 로딩 텍스트
    
    [Header("에러 핸들러")]
    [SerializeField] private ErrorHandler errorHandler; // 에러 핸들러
    
    // 로그인 상태 관리
    private bool _isLoggingIn;

    
    /// <summary>
    /// 필수 컴포넌트들이 제대로 연결되었는지 확인하는 함수
    /// </summary>
    private void ValidateComponents()
    {
        if (googleLoginButton == null)
            Logger.LogWarning($"{GetType()}::구글 로그인 버튼이 연결되지 않았습니다");
            
        if (appleLoginButton == null)
            Logger.LogWarning($"{GetType()}::애플 로그인 버튼이 연결되지 않았습니다");
            
        if (loadingPanel == null)
            Logger.LogWarning($"{GetType()}::로딩 패널이 연결되지 않았습니다");
    }
    
    /// <summary>
    /// 버튼 이벤트를 설정하는 함수
    /// </summary>
    private void SetupButtonEvents()
    {
        // 구글 로그인 버튼
        if (googleLoginButton != null)
        {
            googleLoginButton.onClick.AddListener(OnClickGoogleLogin);
        }
        
        // 애플 로그인 버튼
        if (appleLoginButton != null)
        {
            appleLoginButton.onClick.AddListener(OnClickAppleLogin);
        }
        
        // 게스트 로그인 버튼
        if (guestLoginButton != null)
        {
            guestLoginButton.onClick.AddListener(OnClickGuestLogin);
        }
    }
    
    /// <summary>
    /// Firebase 이벤트를 설정하는 함수
    /// </summary>
    private void SetupFirebaseEvents()
    {
        if (FirebaseManager.Instance != null)
        {
            // 로그인 성공 이벤트
            FirebaseManager.Instance.OnUserSignedIn += OnLoginSuccess;
            
            // 로그인 실패 이벤트
            FirebaseManager.Instance.OnSignInFailed += OnLoginFailed;
        }
        else
        {
            Logger.LogWarning($"{GetType()}::FirebaseManager를 찾을 수 없습니다");
        }
    }

    /// <summary>
    /// UI 설정 시 호출되는 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);
        
        // 필수 컴포넌트 확인
        ValidateComponents();
        
        // 버튼 이벤트 연결
        SetupButtonEvents();
        
        // Firebase 이벤트 연결
        SetupFirebaseEvents();
        
        // UI 초기 상태 설정
        SetupInitialUI();
    }
    
    /// <summary>
    /// UI 초기 상태를 설정하는 함수
    /// </summary>
    private void SetupInitialUI()
    {
        // 제목 설정
        if (titleText != null)
        {
            titleText.text = "학습 도우미에 오신 것을 환영합니다!";
        }
        
        // 설명 설정
        if (descriptionText != null)
        {
            descriptionText.text = "계정으로 로그인하면 학습 기록이 안전하게 저장되고\n다른 기기에서도 이어서 사용할 수 있습니다.";
        }
        
        // 상태 텍스트 초기화
        if (statusText != null)
        {
            statusText.text = "로그인 방법을 선택해주세요";
        }
        
        // 로딩 패널 숨기기
        SetLoadingVisible(false);
        
        // 버튼들 활성화
        SetButtonsEnabled(true);
        
        // 플랫폼별 버튼 표시/숨김 처리
        SetupPlatformSpecificUI();
    }
    
    /// <summary>
    /// 플랫폼별 UI를 설정하는 함수
    /// </summary>
    private void SetupPlatformSpecificUI()
    {
        // iOS에서만 애플 로그인 버튼 표시
        if (appleLoginButton != null)
        {
            #if UNITY_IOS
                appleLoginButton.gameObject.SetActive(true);
            #else
                appleLoginButton.gameObject.SetActive(false);
            #endif
        }
        
        Logger.Log($"{GetType()}::플랫폼별 UI 설정 완료");
    }

    /// <summary>
    /// 구글 로그인 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickGoogleLogin()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (errorHandler != null)
                errorHandler.Show(ErrorType.NetworkError);
            return;
        }
        
        Logger.Log($"{GetType()}::구글 로그인 버튼이 클릭되었습니다");
        
        if (_isLoggingIn)
        {
            Logger.Log($"{GetType()}::이미 로그인 진행 중입니다");
            return;
        }
        
        StartLogin("구글 계정으로 로그인 중...", () => {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.SignInWithGoogle();
            }
            else
            {
                OnLoginFailed("FirebaseManager를 찾을 수 없습니다");
            }
        });
    }

    /// <summary>
    /// 애플 로그인 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickAppleLogin()
    {
        Logger.Log($"{GetType()}::애플 로그인 버튼이 클릭되었습니다");
        
        if (_isLoggingIn)
        {
            Logger.Log($"{GetType()}::이미 로그인 진행 중입니다");
            return;
        }
        
        StartLogin("Apple ID로 로그인 중...", () => {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.SignInWithApple();
            }
            else
            {
                OnLoginFailed("FirebaseManager를 찾을 수 없습니다");
            }
        });
    }
    
    /// <summary>
    /// 게스트 로그인 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickGuestLogin()
    {
        Logger.Log($"{GetType()}::게스트 로그인 버튼이 클릭되었습니다");
        
        if (_isLoggingIn)
        {
            Logger.Log($"{GetType()}::이미 로그인 진행 중입니다");
            return;
        }
        
        // 게스트 로그인 확인 모달 표시
        ShowGuestLoginConfirmation();
    }
    
    /// <summary>
    /// 게스트 로그인 확인 모달을 표시하는 함수
    /// </summary>
    private void ShowGuestLoginConfirmation()
    {
        var modalData = new ModalUIData
        {
            Type = ModalType.OkCancel,
            Title = "게스트로 시작",
            Desc = "게스트로 시작하면 기기에만 데이터가 저장되며,\n앱을 삭제하거나 기기를 바꾸면 데이터가 사라질 수 있습니다.\n\n그래도 게스트로 시작하시겠습니까?",
            OkBtnText = "게스트로 시작",
            CancelBtnText = "취소",
            OkAction = StartGuestLogin,
            CancelAction = () => Logger.Log($"{GetType()}::게스트 로그인이 취소되었습니다")
        };
        
        UIManager.Instance?.OpenUI<ModalUI>(modalData);
    }
    
    /// <summary>
    /// 게스트 로그인을 시작하는 함수
    /// </summary>
    private void StartGuestLogin()
    {
        StartLogin("게스트로 시작 중...", () => {
            // 게스트 로그인 처리 (즉시 성공으로 처리)
            OnLoginSuccess(null);
        });
    }
    
    /// <summary>
    /// 로그인을 시작하는 함수
    /// </summary>
    private void StartLogin(string loadingMessage, System.Action loginAction)
    {
        try
        {
            _isLoggingIn = true;
            
            // UI 상태 변경
            SetLoadingVisible(true, loadingMessage);
            SetButtonsEnabled(false);
            
            // 로그인 실행
            loginAction?.Invoke();
            
            Logger.Log($"{GetType()}::로그인 시작: {loadingMessage}");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::로그인 시작 중 오류: {e.Message}");
            OnLoginFailed($"로그인 시작 중 오류가 발생했습니다: {e.Message}");
        }
    }
    
    /// <summary>
    /// 로그인 성공 시 호출되는 함수
    /// </summary>
    private void OnLoginSuccess(Firebase.Auth.FirebaseUser user)
    {
        Logger.Log($"{GetType()}::로그인 성공");
        
        try
        {
            // 로딩 상태 해제
            _isLoggingIn = false;
            
            // 성공 메시지 표시
            if (statusText != null)
            {
                string userName = user?.DisplayName ?? "게스트";
                statusText.text = $"환영합니다, {userName}님!";
            }
            
            // 잠시 후 UI 닫기
            Invoke(nameof(CloseUIAfterDelay), 1.5f);
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::로그인 성공 처리 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 로그인 실패 시 호출되는 함수
    /// </summary>
    private void OnLoginFailed(string errorMessage)
    {
        Logger.LogError($"{GetType()}::로그인 실패: {errorMessage}");
        
        try
        {
            // 로딩 상태 해제
            _isLoggingIn = false;
            SetLoadingVisible(false);
            SetButtonsEnabled(true);
            
            // 실패 메시지 표시
            if (statusText != null)
            {
                statusText.text = "로그인에 실패했습니다. 다시 시도해주세요.";
            }
            
            // 에러 모달 표시
            ShowErrorModal("로그인 실패", errorMessage);
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::로그인 실패 처리 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 지연 후 UI를 닫는 함수
    /// </summary>
    private void CloseUIAfterDelay()
    {
        CloseUI();
    }
    
    /// <summary>
    /// 로딩 상태를 설정하는 함수
    /// </summary>
    private void SetLoadingVisible(bool visible, string message = "")
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(visible);
        }
        
        if (loadingText != null && visible)
        {
            loadingText.text = message;
        }
    }
    
    /// <summary>
    /// 버튼들의 활성화 상태를 설정하는 함수
    /// </summary>
    private void SetButtonsEnabled(bool _enabled)
    {
        if (googleLoginButton != null)
            googleLoginButton.interactable = _enabled;
            
        if (appleLoginButton != null)
            appleLoginButton.interactable = _enabled;
            
        if (guestLoginButton != null)
            guestLoginButton.interactable = _enabled;
    }
    
    /// <summary>
    /// 에러 모달을 표시하는 함수
    /// </summary>
    private void ShowErrorModal(string title, string message)
    {
        try
        {
            var modalData = new ModalUIData
            {
                Type = ModalType.Ok,
                Title = title,
                Desc = message,
                OkBtnText = "확인"
            };
            
            UIManager.Instance?.OpenUI<ModalUI>(modalData);
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::에러 모달 표시 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// UI가 닫힐 때 호출되는 함수
    /// </summary>
    protected override void OnClose()
    {
        base.OnClose();
        
        // Firebase 이벤트 해제
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnUserSignedIn -= OnLoginSuccess;
            FirebaseManager.Instance.OnSignInFailed -= OnLoginFailed;
        }
        
        // 버튼 이벤트 해제
        if (googleLoginButton != null)
            googleLoginButton.onClick.RemoveAllListeners();
            
        if (appleLoginButton != null)
            appleLoginButton.onClick.RemoveAllListeners();
            
        if (guestLoginButton != null)
            guestLoginButton.onClick.RemoveAllListeners();
        
        Logger.Log($"{GetType()}::AccountUI가 정리되었습니다");
    }
    
    /// <summary>
    /// 뒤로가기 키 처리 (앱 종료 확인)
    /// </summary>
    protected override void OnBackKeyPressed()
    {
        var modalData = new ModalUIData
        {
            Type = ModalType.OkCancel,
            Title = "앱 종료",
            Desc = "정말 앱을 종료하시겠습니까?",
            OkBtnText = "종료",
            CancelBtnText = "취소",
            OkAction = Application.Quit,
            CancelAction = () => Logger.Log($"{GetType()}::앱 종료가 취소되었습니다")
        };
        
        UIManager.Instance?.OpenUI<ModalUI>(modalData);
    }
}