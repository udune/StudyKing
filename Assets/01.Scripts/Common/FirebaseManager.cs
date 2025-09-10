using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using Google;
using UnityEngine;
using Logger = Common.Logger;

public class FirebaseManager : SingletonBehaviour<FirebaseManager>, IFirebaseManager
{
    [Header("Configuration")]
    [SerializeField] private float initTimeout = 10.0f;
    
    // Remote Config
    private FirebaseRemoteConfig remoteConfig;
    private bool isRemoteConfigInit;
    private readonly Dictionary<string, object> remoteConfigDic = new Dictionary<string, object>();
    
    // Auth
    public FirebaseAuth auth;
    private bool isAuthInit = false;
    private GoogleSignInConfiguration googleSignInConfiguration;
    private FirebaseUser firebaseUser;
    
    // Firestore
    private FirebaseFirestore database;
    private bool isFirestoreInit = false;
    
    // Analytics
    private bool isAnalyticsInit = false;

    public bool HasSignedWithGoogle { get; private set; }
    public bool HasSignedWithApple { get; private set; }

    public event Action OnInitialized;
    public event Action<FirebaseUser> OnUserSignedIn;
    public event Action OnUserSignedOut;
    public event Action<string> OnSignInFailed;

    protected override void Init()
    {
        base.Init();
        
        LoadData();
        StartCoroutine(InitFirebaseServiceCoroutine());
    }

    protected override void OnDestroy()
    {
        try
        {
            // Firebase RemoteConfig 정리
            if (remoteConfig != null)
            {
                // RemoteConfig는 자동으로 정리되므로 null로 설정만 함
                remoteConfig = null;
                isRemoteConfigInit = false;
            }

            // Auth 정리
            if (auth != null)
            {
                auth.StateChanged -= OnAuthStateChanged; // 중요: 이벤트 해제 추가
                auth = null;
                isAuthInit = false;
            }

            // Firestore 정리
            if (database != null)
            {
                database = null;
                isFirestoreInit = false;
            }

            // 이벤트 정리
            OnInitialized = null;
            OnUserSignedIn = null;
            OnUserSignedOut = null;
            OnSignInFailed = null;

            Logger.Log($"{GetType()}::Firebase 리소스 정리 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::OnDestroy Exception: {e}");
        }
        finally
        {
            base.OnDestroy();
        }
    }

    private void LoadData()
    {
        try
        {
            HasSignedWithGoogle = PlayerPrefs.GetInt(Constants.PlayerPrefs.HAS_SIGNED_WITH_GOOGLE) == 1;
            HasSignedWithApple = PlayerPrefs.GetInt(Constants.PlayerPrefs.HAS_SIGNED_WITH_APPLE) == 1;
            Logger.Log($"{GetType()}::HasSignedWithGoogle: {HasSignedWithGoogle}");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::LoadData Exception: {e}");
        }
    }

    private void SaveData()
    {
        try
        {
            PlayerPrefs.SetInt(Constants.PlayerPrefs.HAS_SIGNED_WITH_GOOGLE, HasSignedWithGoogle ? 1 : 0);
            PlayerPrefs.SetInt(Constants.PlayerPrefs.HAS_SIGNED_WITH_APPLE, HasSignedWithApple ? 1 : 0);
            PlayerPrefs.Save();
            Logger.Log($"{GetType()}::SaveData Success");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SaveData Exception: {e}");
        }
    }

    private IEnumerator InitFirebaseServiceCoroutine()
    {
        Logger.Log($"{GetType()}::FirebaseApp initialization start.");
        
        var checkTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);
        
        var dependencyStatus = checkTask.Result;
        if (dependencyStatus != DependencyStatus.Available)
        {
            Logger.LogError($"{GetType()}::Firebase dependency failed: {dependencyStatus}");
            OnInitialized?.Invoke();
            yield break;
        }
        
        Logger.Log($"{GetType()}::Firebase dependencies resolved successfully");

        // RemoteConfig 초기화
        InitRemoteConfig();
        
        // Auth 초기화
        InitAuth();
        
        // Firestore 초기화  
        InitFirestore();
        
        // Analytics 초기화
        InitAnalytics();
        
        // 초기화 완료 대기
        yield return new WaitUntil(() => IsInit());
        
        Logger.Log($"{GetType()}::Firebase initialization complete");
        OnInitialized?.Invoke();
    }
    
    #region REMOTE_CONFIG

    private void InitRemoteConfig()
    {
        try
        {
            remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            if (remoteConfig == null)
            {
                Logger.LogError($"{GetType()}::FirebaseApp Initialization failed. FirebaseRemoteConfig is null.");
                SetDefaultRemoteConfig();
                return;
            }

            var defaults = new Dictionary<string, object>
            {
                { "dev_app_version", Application.version },
                { "real_app_version", Application.version },
                { "openai_apikey", string.Empty }
            };

            remoteConfig.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    FetchRemoteConfig();
                }
                else
                {
                    Logger.LogError($"{GetType()}::RemoteConfig SetDefaultsAsync failed");
                    SetDefaultRemoteConfig();
                }
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::RemoteConfig Exception: {e}");
            SetDefaultRemoteConfig();
        }
    }

    private void SetDefaultRemoteConfig()
    {
        remoteConfigDic["dev_app_version"] = Application.version;
        remoteConfigDic["real_app_version"] = Application.version;
        remoteConfigDic["openai_apikey"] = string.Empty;
        isRemoteConfigInit = true;
        Logger.Log($"{GetType()}::RemoteConfig set to default values.");
    }

    private void FetchRemoteConfig()
    {
        remoteConfig.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(fetchTask =>
        {
            if (fetchTask.IsCompletedSuccessfully)
            {
                remoteConfig.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    if (activateTask.IsCompletedSuccessfully)
                    {
                        UpdateRemoteConfigValues();
                        isRemoteConfigInit = true;
                        Logger.Log($"{GetType()}::RemoteConfig ActivateAsync success");
                    }
                    else
                    {
                        Logger.LogError($"{GetType()}::RemoteConfig ActivateAsync failed");
                        SetDefaultRemoteConfig();
                    }
                });
            }
            else
            {
                Logger.LogError($"{GetType()}::RemoteConfig FetchAsync failed");
                SetDefaultRemoteConfig();
            }
        });
    }

    private void UpdateRemoteConfigValues()
    {
        try
        {
            remoteConfigDic["dev_app_version"] = remoteConfig.GetValue("dev_app_version").StringValue;
            remoteConfigDic["real_app_version"] = remoteConfig.GetValue("real_app_version").StringValue;
            remoteConfigDic["openai_apikey"] = remoteConfig.GetValue("openai_apikey").StringValue;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UpdateRemoteConfigValues Exception: {e}");
        }
    }

    public string GetAppVersion()
    {
        try
        {
#if DEV_VER
            return remoteConfigDic.TryGetValue("dev_app_version", out var value) ? 
                   (string.IsNullOrEmpty(value.ToString()) ? Application.version : value.ToString()) : Application.version;
#else
            return remoteConfigDic.TryGetValue("real_app_version", out var value) ? 
                   (string.IsNullOrEmpty(value.ToString()) ? Application.version : value.ToString()) : Application.version;
#endif
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::GetAppVersion Exception: {e}");
            return Application.version;
        }
    }

    public string GetOpenAIKey()
    {
        try
        {
            return remoteConfigDic.TryGetValue("openai_apikey", out var value) ? value.ToString() : string.Empty;
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::GetOpenAIKey Exception: {e}");
            return string.Empty;
        }
    }
    
    #endregion
    
    #region AUTH

    private void InitAuth()
    {
        try
        {
            auth = FirebaseAuth.DefaultInstance;
            if (auth == null)
            {
                Logger.LogError($"{GetType()}::FirebaseAuth is null");
                isAuthInit = true;
                return;
            }

            // Google Sign-In 구성 설정
            googleSignInConfiguration = new GoogleSignInConfiguration()
            {
                WebClientId = Constants.Firebase.GOOGLE_WEB_CLIENT_ID,
                RequestIdToken = true
            };
            
            // Google Sign-In 구성 적용
            GoogleSignIn.Configuration = googleSignInConfiguration;
            
            auth.StateChanged += OnAuthStateChanged;
            isAuthInit = true;
            
            Logger.Log($"{GetType()}::Auth initialization success");

            // 자동 로그인 처리
            HandleAutoSignIn();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::InitAuth Exception: {e}");
            isAuthInit = true;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        try
        {
            if (SceneLoader.Instance.GetCurrentScene() == SceneType.Title)
            {
                return;
            }

            if (auth?.CurrentUser == null)
            {
                Logger.Log($"{GetType()}::User Signed out or disconnected");
                HandleSignOut();
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::OnAuthStateChanged Exception: {e}");
        }
    }

    private void HandleAutoSignIn()
    {
        try
        {
            if (auth.CurrentUser != null)
            {
                firebaseUser = auth.CurrentUser;
                Logger.Log($"{GetType()}::User already signed in: {firebaseUser.DisplayName}");
                OnUserSignedIn?.Invoke(firebaseUser);
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::HandleAutoSignIn Exception: {e}");
        }
    }

    private void HandleSignOut()
    {
        firebaseUser = null;
        HasSignedWithGoogle = false;
        HasSignedWithApple = false;
        SaveData();
        
        OnUserSignedOut?.Invoke();
        
        UIManager.Instance.CloseAllOpenUI();
        SceneLoader.Instance.LoadScene(SceneType.Title);
    }
    
    public bool IsInit()
    {
        return isRemoteConfigInit && isAuthInit && isFirestoreInit && isAnalyticsInit;
    }

    public bool IsSignedIn()
    {
#if UNITY_EDITOR
        return true;
#else
        return firebaseUser != null;
#endif
    }

    public void SignInWithGoogle()
    {
        Logger.Log($"{GetType()}::SignInWithGoogle 시작");
        
        // 초기화 확인
        if (!isAuthInit || auth == null)
        {
            Logger.LogError($"{GetType()}::Auth가 초기화되지 않았습니다");
            OnSignInFailed?.Invoke("인증 시스템이 준비되지 않았습니다");
            return;
        }

        if (googleSignInConfiguration == null)
        {
            Logger.LogError($"{GetType()}::Google 구성이 없습니다");
            OnSignInFailed?.Invoke("Google 로그인 구성 오류");
            return;
        }

        try
        {
            // Google Sign-In 구성 재설정 (안전하게)
            GoogleSignIn.Configuration = googleSignInConfiguration;
            
            Logger.Log($"{GetType()}::Google Sign-In 시작...");
            
            GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Logger.Log($"{GetType()}::Google Sign-In 취소됨");
                    return;
                }
                
                if (task.IsFaulted)
                {
                    string errorMsg = task.Exception?.GetBaseException()?.Message ?? "알 수 없는 오류";
                    Logger.LogError($"{GetType()}::Google Sign-In 실패: {errorMsg}");
                    OnSignInFailed?.Invoke($"Google 로그인 실패: {errorMsg}");
                    return;
                }

                var googleUser = task.Result;
                if (googleUser == null)
                {
                    Logger.LogError($"{GetType()}::Google 사용자 정보가 null입니다");
                    OnSignInFailed?.Invoke("Google 사용자 정보를 가져올 수 없습니다");
                    return;
                }
                
                Logger.Log($"{GetType()}::Google Sign-In 성공: {googleUser.DisplayName}");
                
                // Firebase 인증
                var credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                SignInWithCredential(credential, true, false);
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SignInWithGoogle Exception: {e}");
            OnSignInFailed?.Invoke($"Google 로그인 오류: {e.Message}");
        }
    }

    public void SignInWithApple()
    {
        Logger.Log($"{GetType()}::Apple Sign-In은 아직 구현되지 않았습니다");
        OnSignInFailed?.Invoke("Apple Sign-In은 준비 중입니다");
    }

    private void SignInWithCredential(Credential credential, bool isGoogle, bool isApple)
    {
        if (auth == null)
        {
            Logger.LogError($"{GetType()}::Auth가 null입니다");
            OnSignInFailed?.Invoke("인증 시스템 오류");
            return;
        }

        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string errorMessage = task.IsCanceled ? "로그인이 취소되었습니다." : $"로그인 실패: {task.Exception?.GetBaseException()?.Message}";
                Logger.LogError($"{GetType()}::Firebase 인증 실패: {errorMessage}");
                OnSignInFailed?.Invoke(errorMessage);
                return;
            }

            firebaseUser = task.Result;
            if (firebaseUser == null)
            {
                Logger.LogError($"{GetType()}::Firebase 사용자가 null입니다");
                OnSignInFailed?.Invoke("Firebase 인증 오류");
                return;
            }

            // 로그인 상태 저장
            HasSignedWithGoogle = isGoogle;
            HasSignedWithApple = isApple;
            SaveData();

            Logger.Log($"{GetType()}::Firebase 로그인 성공: {firebaseUser.DisplayName}");
            OnUserSignedIn?.Invoke(firebaseUser);
        });
    }

    public void SignOut()
    {
        try
        {
            if (auth != null)
            {
                auth.SignOut();
            }
            
            GoogleSignIn.DefaultInstance.SignOut();
            HandleSignOut();
            
            Logger.Log($"{GetType()}::로그아웃 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SignOut Exception: {e}");
        }
    }
    
    private string GetUserId()
    {
#if UNITY_EDITOR
        return Constants.Firebase.UNITY_EDITOR_USER_ID;
#else
        return firebaseUser?.UserId ?? string.Empty;
#endif
    }
    
    #endregion
    
    #region FIRESTORE
    
    private void InitFirestore()
    {
        try
        {
            database = FirebaseFirestore.DefaultInstance;
            isFirestoreInit = true;
            Logger.Log($"{GetType()}::Firestore initialization success");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::InitFirestore Exception: {e}");
            isFirestoreInit = true;
        }
    }

    public void LoadUserData<T>(Action onFinishLoad = null) where T : class, IUserData
    {
        if (!isFirestoreInit)
        {
            Logger.LogError($"{GetType()}::Firestore is not initialized");
            onFinishLoad?.Invoke();
            return;
        }
        
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Logger.LogError($"{GetType()}::User ID is null or empty");
            onFinishLoad?.Invoke();
            return;
        }

        try
        {
            var type = typeof(T);
            database.Collection($"{type}").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread<DocumentSnapshot>(task =>
            {
                try
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        var userData = UserDataManager.Instance.GetUserData<T>();
                        if (userData == null)
                        {
                            Logger.LogError($"{GetType()}::UserData is null");
                            onFinishLoad?.Invoke();
                            return;
                        }
                        
                        var snapshot = task.Result;
                        if (snapshot != null && snapshot.Exists)
                        {
                            Logger.Log($"{GetType()}::{type} Loaded Successfully");
                            var userDataDict = snapshot.ToDictionary();
                            userData.Setting(userDataDict);
                        }
                        else
                        {
                            Logger.Log($"{GetType()}::{type} No Found. Create new data");
                            userData.Initialize();
                            userData.SaveData();
                        }
                    }
                    else
                    {
                        Logger.LogError($"{GetType()}::{type} Loading Failed");
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"{GetType()}::LoadUserData inner Exception: {e}");
                }
                finally
                {
                    onFinishLoad?.Invoke();
                }
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::LoadUserData Exception: {e}");
            onFinishLoad?.Invoke();
        }
    }

    // 인터페이스 구현: Dictionary를 받는 SaveUserData
    public void SaveUserData<T>(Dictionary<string, object> userDataDict) where T : class, IUserData
    {
        if (!isFirestoreInit)
        {
            Logger.LogError($"{GetType()}::Firestore is not initialized");
            return;
        }

        if (userDataDict == null)
        {
            Logger.LogError($"{GetType()}::userDataDict is null");
            return;
        }

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Logger.LogError($"{GetType()}::User ID is null or empty");
            return;
        }

        try
        {
            var type = typeof(T);
            Logger.Log($"Attempting to save {type} data for user: {userId}");
            Logger.Log($"Data to save: {string.Join(", ", userDataDict.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            
            var documentReference = database.Collection($"{type}").Document(userId);
            documentReference.SetAsync(userDataDict).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Logger.Log($"{type} saved Successfully");
                }
                else
                {
                    var errorMessage = "Unknown error";
                    if (task.Exception != null)
                    {
                        errorMessage = task.Exception.GetBaseException().Message;
                        Logger.LogError($"{type} saving Failed: {errorMessage}");
                        Logger.LogError($"Full exception: {task.Exception}");
                    }
                    else if (task.IsCanceled)
                    {
                        Logger.LogError($"{type} saving Failed: Task was canceled");
                    }
                    else if (task.IsFaulted)
                    {
                        Logger.LogError($"{type} saving Failed: Task was faulted");
                    }
                    else
                    {
                        Logger.LogError($"{type} saving Failed: {errorMessage}");
                    }
                }
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SaveUserData Exception: {e}");
        }
    }
    
    #endregion
    
    #region ANALYTICS

    private void InitAnalytics()
    {
        try
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            isAnalyticsInit = true;
            Logger.Log($"{GetType()}::FirebaseAnalytics Initialization success");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::FirebaseAnalytics Exception: {e}");
            isAnalyticsInit = true;
        }
    }

    // 인터페이스 구현: LogCustomEvent
    public void LogCustomEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (!isAnalyticsInit)
        {
            Logger.LogWarning($"{GetType()}::Analytics is not initialized");
            return;
        }

        if (string.IsNullOrEmpty(eventName))
        {
            Logger.LogError($"{GetType()}::Event name is null or empty");
            return;
        }

        try
        {
            var firebaseParameters = new List<Parameter>();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param.Value != null)
                    {
                        firebaseParameters.Add(new Parameter(param.Key, param.Value.ToString()));
                    }
                }
            }
            
            FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());
            Logger.Log($"{GetType()}::Custom event {eventName} logged");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::LogCustomEvent Exception: {e}");
        }
    }
    
    #endregion

    protected override void Dispose()
    {
        try
        {
            // Auth 이벤트 해제
            if (auth != null)
            {
                auth.StateChanged -= OnAuthStateChanged;
            }
            
            Logger.Log($"{GetType()}::FirebaseManager Dispose 완료");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::Dispose Exception: {e}");
        }
        finally
        {
            base.Dispose();
        }
    }
}