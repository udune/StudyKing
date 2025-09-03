using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Logger = Common.Logger;

/// <summary>
/// 내 설정 탭 UI 클래스
/// 사용자 정보, 앱 설정, 로그아웃 등의 기능을 제공합니다
/// </summary>
public class MySettingTabUI : BaseUI
{
    [Header("사용자 정보")]
    [SerializeField] private TMP_Text userNameText;       // 사용자 이름 텍스트
    [SerializeField] private TMP_Text userEmailText;      // 사용자 이메일 텍스트
    [SerializeField] private Image userProfileImage;      // 사용자 프로필 이미지
    
    [Header("앱 정보")]
    [SerializeField] private TMP_Text versionText;        // 앱 버전 텍스트
    [SerializeField] private TMP_Text buildNumberText;    // 빌드 번호 텍스트
    
    [Header("설정 버튼들")]
    [SerializeField] private Button logoutButton;         // 로그아웃 버튼
    [SerializeField] private Button dataResetButton;      // 데이터 초기화 버튼
    [SerializeField] private Button feedbackButton;       // 피드백 버튼
    [SerializeField] private Button privacyPolicyButton;  // 개인정보처리방침 버튼
    [SerializeField] private Button termsOfServiceButton; // 이용약관 버튼
    
    [Header("설정 토글들")]
    [SerializeField] private Toggle notificationToggle;   // 알림 설정 토글
    [SerializeField] private Toggle soundToggle;          // 사운드 설정 토글
    [SerializeField] private Toggle vibrationToggle;      // 진동 설정 토글
    
    [Header("UI 섹션들")]
    [SerializeField] private GameObject userInfoSection;   // 사용자 정보 섹션
    [SerializeField] private GameObject appInfoSection;    // 앱 정보 섹션
    [SerializeField] private GameObject settingsSection;   // 설정 섹션

    // 현재 사용자 정보
    private Firebase.Auth.FirebaseUser _currentUser;
    
    // 설정 데이터
    private UserSettingsData _userSettings;

    /// <summary>
    /// UI 초기화 시 호출되는 함수
    /// </summary>
    protected override void OnInit()
    {
        base.OnInit();
        
        // 컴포넌트 검증
        ValidateComponents();
        
        // 버튼 이벤트 설정
        SetupButtonEvents();
        
        // 토글 이벤트 설정
        SetupToggleEvents();
    }
    
    /// <summary>
    /// 필수 컴포넌트들이 제대로 연결되었는지 확인하는 함수
    /// </summary>
    private void ValidateComponents()
    {
        if (versionText == null)
            Logger.LogWarning($"{GetType()}::버전 텍스트가 연결되지 않았습니다");
            
        if (logoutButton == null)
            Logger.LogWarning($"{GetType()}::로그아웃 버튼이 연결되지 않았습니다");
    }
    
    /// <summary>
    /// 버튼 이벤트를 설정하는 함수
    /// </summary>
    private void SetupButtonEvents()
    {
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnClickLogout);
            
        if (dataResetButton != null)
            dataResetButton.onClick.AddListener(OnClickDataReset);
            
        if (feedbackButton != null)
            feedbackButton.onClick.AddListener(OnClickFeedback);
            
        if (privacyPolicyButton != null)
            privacyPolicyButton.onClick.AddListener(OnClickPrivacyPolicy);
            
        if (termsOfServiceButton != null)
            termsOfServiceButton.onClick.AddListener(OnClickTermsOfService);
    }
    
    /// <summary>
    /// 토글 이벤트를 설정하는 함수
    /// </summary>
    private void SetupToggleEvents()
    {
        if (notificationToggle != null)
            notificationToggle.onValueChanged.AddListener(OnNotificationToggleChanged);
            
        if (soundToggle != null)
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
    }

    /// <summary>
    /// UI 설정 시 호출되는 함수
    /// </summary>
    protected override void OnSetting(BaseUIData data)
    {
        base.OnSetting(data);
        
        // 사용자 정보 로드
        LoadUserInfo();
        
        // 앱 정보 설정
        SetupAppInfo();
        
        // 사용자 설정 로드
        LoadUserSettings();
    }
    
    /// <summary>
    /// 사용자 정보를 로드하는 함수
    /// </summary>
    private void LoadUserInfo()
    {
        try
        {
            // Firebase에서 현재 사용자 정보 가져오기
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.auth.CurrentUser != null)
            {
                _currentUser = FirebaseManager.Instance.auth.CurrentUser;
                DisplayUserInfo();
            }
            else
            {
                DisplayGuestInfo();
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::사용자 정보 로드 중 오류: {e.Message}");
            DisplayGuestInfo();
        }
    }
    
    /// <summary>
    /// 사용자 정보를 화면에 표시하는 함수
    /// </summary>
    private void DisplayUserInfo()
    {
        if (_currentUser == null) return;
        
        // 사용자 이름 설정
        if (userNameText != null)
        {
            string displayName = _currentUser.DisplayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "사용자";
            }
            userNameText.text = displayName;
        }
        
        // 사용자 이메일 설정
        if (userEmailText != null)
        {
            string email = _currentUser.Email;
            if (string.IsNullOrEmpty(email))
            {
                email = "이메일 없음";
            }
            userEmailText.text = email;
        }
        
        // 프로필 이미지 설정 (추후 구현)
        SetupProfileImage();
        
        // 로그아웃 버튼 활성화
        if (logoutButton != null)
        {
            logoutButton.gameObject.SetActive(true);
        }
        
        Logger.Log($"{GetType()}::사용자 정보 표시 완료 - {_currentUser.DisplayName}");
    }
    
    /// <summary>
    /// 게스트 사용자 정보를 표시하는 함수
    /// </summary>
    private void DisplayGuestInfo()
    {
        if (userNameText != null)
            userNameText.text = "게스트 사용자";
            
        if (userEmailText != null)
            userEmailText.text = "로그인하지 않음";
            
        // 로그아웃 버튼 비활성화
        if (logoutButton != null)
        {
            logoutButton.gameObject.SetActive(false);
        }
        
        Logger.Log($"{GetType()}::게스트 사용자 정보 표시");
    }
    
    /// <summary>
    /// 프로필 이미지를 설정하는 함수
    /// </summary>
    private void SetupProfileImage()
    {
        if (userProfileImage == null || _currentUser == null) return;
        
        // 기본 프로필 이미지 설정
        // TODO: Firebase에서 프로필 이미지 URL을 가져와서 로드하는 기능 구현
        Logger.Log($"{GetType()}::프로필 이미지 설정 (추후 구현)");
    }
    
    /// <summary>
    /// 앱 정보를 설정하는 함수
    /// </summary>
    private void SetupAppInfo()
    {
        // 앱 버전 설정
        if (versionText != null)
        {
            versionText.text = $"버전: {Application.version}";
        }
        
        // 빌드 번호 설정 (Android/iOS별로 다를 수 있음)
        if (buildNumberText != null)
        {
            #if UNITY_ANDROID
                buildNumberText.text = $"빌드: {Application.version}";
            #elif UNITY_IOS
                buildNumberText.text = $"빌드: {Application.version}";
            #else
                buildNumberText.text = $"빌드: {Application.version}";
            #endif
        }
        
        Logger.Log($"{GetType()}::앱 정보 설정 완료 - 버전: {Application.version}");
    }
    
    /// <summary>
    /// 사용자 설정을 로드하는 함수
    /// </summary>
    private void LoadUserSettings()
    {
        try
        {
            // 사용자 설정 데이터 로드
            _userSettings = UserDataManager.Instance?.GetUserData<UserSettingsData>();
            
            if (_userSettings == null)
            {
                // 기본 설정 생성
                CreateDefaultSettings();
            }
            
            // UI에 설정 반영
            ApplySettingsToUI();
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::사용자 설정 로드 중 오류: {e.Message}");
            CreateDefaultSettings();
        }
    }
    
    /// <summary>
    /// 기본 설정을 생성하는 함수
    /// </summary>
    private void CreateDefaultSettings()
    {
        _userSettings = new UserSettingsData
        {
            notificationEnabled = true,
            soundEnabled = true,
            vibrationEnabled = true
        };
        
        _userSettings.SaveData();
        Logger.Log($"{GetType()}::기본 설정을 생성했습니다");
    }
    
    /// <summary>
    /// 설정을 UI에 반영하는 함수
    /// </summary>
    private void ApplySettingsToUI()
    {
        if (_userSettings == null) return;
        
        if (notificationToggle != null)
            notificationToggle.isOn = _userSettings.notificationEnabled;
            
        if (soundToggle != null)
            soundToggle.isOn = _userSettings.soundEnabled;
            
        if (vibrationToggle != null)
            vibrationToggle.isOn = _userSettings.vibrationEnabled;
    }

    #region 버튼 이벤트 함수들
    
    /// <summary>
    /// 로그아웃 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickLogout()
    {
        Logger.Log($"{GetType()}::로그아웃 버튼이 클릭되었습니다");
        
        // 로그아웃 확인 모달 표시
        ShowLogoutConfirmation();
    }
    
    /// <summary>
    /// 로그아웃 확인 모달을 표시하는 함수
    /// </summary>
    private void ShowLogoutConfirmation()
    {
        var modalData = new ModalUIData
        {
            Type = ModalType.OkCancel,
            Title = "로그아웃",
            Desc = "정말 로그아웃하시겠습니까?\n로컬에 저장된 데이터는 유지됩니다.",
            OkBtnText = "로그아웃",
            CancelBtnText = "취소",
            OkAction = PerformLogout,
            CancelAction = () => Logger.Log($"{GetType()}::로그아웃이 취소되었습니다")
        };
        
        UIManager.Instance?.OpenUI<ModalUI>(modalData);
    }
    
    /// <summary>
    /// 실제 로그아웃을 수행하는 함수
    /// </summary>
    private void PerformLogout()
    {
        try
        {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.SignOut();
                Logger.Log($"{GetType()}::로그아웃 완료");
                
                // UI 새로고침
                LoadUserInfo();
            }
            else
            {
                Logger.LogWarning($"{GetType()}::FirebaseManager를 찾을 수 없습니다");
            }
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::로그아웃 중 오류: {e.Message}");
            ShowErrorModal("오류", "로그아웃 중 문제가 발생했습니다.");
        }
    }
    
    /// <summary>
    /// 데이터 초기화 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickDataReset()
    {
        Logger.Log($"{GetType()}::데이터 초기화 버튼이 클릭되었습니다");
        
        ShowDataResetConfirmation();
    }
    
    /// <summary>
    /// 데이터 초기화 확인 모달을 표시하는 함수
    /// </summary>
    private void ShowDataResetConfirmation()
    {
        var modalData = new ModalUIData
        {
            Type = ModalType.OkCancel,
            Title = "⚠️ 데이터 초기화",
            Desc = "정말 모든 학습 데이터를 초기화하시겠습니까?\n이 작업은 되돌릴 수 없습니다!",
            OkBtnText = "초기화",
            CancelBtnText = "취소",
            OkAction = PerformDataReset,
            CancelAction = () => Logger.Log($"{GetType()}::데이터 초기화가 취소되었습니다")
        };
        
        UIManager.Instance?.OpenUI<ModalUI>(modalData);
    }
    
    /// <summary>
    /// 실제 데이터 초기화를 수행하는 함수
    /// </summary>
    private void PerformDataReset()
    {
        try
        {
            // 사용자 데이터 모두 삭제
            UserDataManager.Instance?.ClearAllUserData();
            
            // 설정도 초기화
            CreateDefaultSettings();
            ApplySettingsToUI();
            
            Logger.Log($"{GetType()}::데이터 초기화 완료");
            
            ShowInfoModal("완료", "모든 데이터가 초기화되었습니다.");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"{GetType()}::데이터 초기화 중 오류: {e.Message}");
            ShowErrorModal("오류", "데이터 초기화 중 문제가 발생했습니다.");
        }
    }
    
    /// <summary>
    /// 피드백 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickFeedback()
    {
        Logger.Log($"{GetType()}::피드백 버튼이 클릭되었습니다");
        
        // 이메일 앱으로 피드백 보내기
        string email = "feedback@studyapp.com";
        string subject = $"학습 도우미 피드백 (v{Application.version})";
        string body = "안녕하세요!\n\n피드백 내용을 작성해주세요:\n\n";
        
        string emailUrl = $"mailto:{email}?subject={System.Uri.EscapeDataString(subject)}&body={System.Uri.EscapeDataString(body)}";
        Application.OpenURL(emailUrl);
    }
    
    /// <summary>
    /// 개인정보처리방침 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickPrivacyPolicy()
    {
        Logger.Log($"{GetType()}::개인정보처리방침 버튼이 클릭되었습니다");
        
        // 개인정보처리방침 웹페이지 열기
        string privacyPolicyUrl = "https://your-website.com/privacy-policy";
        Application.OpenURL(privacyPolicyUrl);
    }
    
    /// <summary>
    /// 이용약관 버튼 클릭 시 호출되는 함수
    /// </summary>
    private void OnClickTermsOfService()
    {
        Logger.Log($"{GetType()}::이용약관 버튼이 클릭되었습니다");
        
        // 이용약관 웹페이지 열기
        string termsUrl = "https://your-website.com/terms-of-service";
        Application.OpenURL(termsUrl);
    }
    
    #endregion

    #region 토글 이벤트 함수들
    
    /// <summary>
    /// 알림 토글 변경 시 호출되는 함수
    /// </summary>
    private void OnNotificationToggleChanged(bool isOn)
    {
        Logger.Log($"{GetType()}::알림 설정 변경: {isOn}");
        
        if (_userSettings != null)
        {
            _userSettings.notificationEnabled = isOn;
            _userSettings.SaveData();
            
            // 시스템 알림 설정 적용
            ApplyNotificationSettings(isOn);
        }
    }
    
    /// <summary>
    /// 사운드 토글 변경 시 호출되는 함수
    /// </summary>
    private void OnSoundToggleChanged(bool isOn)
    {
        Logger.Log($"{GetType()}::사운드 설정 변경: {isOn}");
        
        if (_userSettings != null)
        {
            _userSettings.soundEnabled = isOn;
            _userSettings.SaveData();
            
            // 시스템 사운드 설정 적용
            ApplySoundSettings(isOn);
        }
    }
    
    /// <summary>
    /// 진동 토글 변경 시 호출되는 함수
    /// </summary>
    private void OnVibrationToggleChanged(bool isOn)
    {
        Logger.Log($"{GetType()}::진동 설정 변경: {isOn}");
        
        if (_userSettings != null)
        {
            _userSettings.vibrationEnabled = isOn;
            _userSettings.SaveData();
            
            // 시스템 진동 설정 적용
            ApplyVibrationSettings(isOn);
        }
    }
    
    #endregion

    #region 설정 적용 함수들
    
    /// <summary>
    /// 알림 설정을 시스템에 적용하는 함수
    /// </summary>
    private void ApplyNotificationSettings(bool _enabled)
    {
        // TODO: 푸시 알림 설정 적용
        Logger.Log($"{GetType()}::알림 설정 적용: {_enabled}");
    }
    
    /// <summary>
    /// 사운드 설정을 시스템에 적용하는 함수
    /// </summary>
    private void ApplySoundSettings(bool _enabled)
    {
        // AudioListener 볼륨 조절
        AudioListener.volume = _enabled ? 1f : 0f;
        Logger.Log($"{GetType()}::사운드 설정 적용: {_enabled}");
    }
    
    /// <summary>
    /// 진동 설정을 시스템에 적용하는 함수
    /// </summary>
    private void ApplyVibrationSettings(bool _enabled)
    {
        // TODO: 진동 설정 적용
        Logger.Log($"{GetType()}::진동 설정 적용: {_enabled}");
    }
    
    #endregion

    #region 유틸리티 함수들
    
    /// <summary>
    /// 정보 모달을 표시하는 함수
    /// </summary>
    private void ShowInfoModal(string title, string message)
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
            Logger.LogError($"{GetType()}::정보 모달 표시 중 오류: {e.Message}");
        }
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
    
    #endregion
    
    /// <summary>
    /// UI가 닫힐 때 호출되는 함수
    /// </summary>
    protected override void OnClose()
    {
        base.OnClose();
        
        // 버튼 이벤트 해제
        if (logoutButton != null)
            logoutButton.onClick.RemoveAllListeners();
            
        if (dataResetButton != null)
            dataResetButton.onClick.RemoveAllListeners();
            
        if (feedbackButton != null)
            feedbackButton.onClick.RemoveAllListeners();
            
        if (privacyPolicyButton != null)
            privacyPolicyButton.onClick.RemoveAllListeners();
            
        if (termsOfServiceButton != null)
            termsOfServiceButton.onClick.RemoveAllListeners();
        
        // 토글 이벤트 해제
        if (notificationToggle != null)
            notificationToggle.onValueChanged.RemoveAllListeners();
            
        if (soundToggle != null)
            soundToggle.onValueChanged.RemoveAllListeners();
            
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveAllListeners();
        
        Logger.Log($"{GetType()}::MySettingTabUI가 정리되었습니다");
    }
}

/// <summary>
/// 사용자 설정 데이터 클래스
/// </summary>
[System.Serializable]
public class UserSettingsData : IUserData
{
    public bool notificationEnabled = true;  // 알림 설정
    public bool soundEnabled = true;         // 사운드 설정
    public bool vibrationEnabled = true;     // 진동 설정

    public bool IsLoaded { get; set; }
    public void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
        throw new System.NotImplementedException();
    }

    public void LoadData()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 설정 데이터를 저장하는 함수
    /// </summary>
    public void SaveData()
    {
        // UserDataManager를 통해 저장
        UserDataManager.Instance?.SaveUserSettingData(this);
    }
}