using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using Google;
using UnityEngine;
using Logger = Common.Logger;

public class FirebaseManager : SingletonBehaviour<FirebaseManager>
{
    private FirebaseApp app;
    
    // Remote Config
    private FirebaseRemoteConfig remoteConfig;
    private bool isRemoteConfigInit;
    private Dictionary<string, object> remoteConfigDic = new Dictionary<string, object>();
    
    // Auth
    private FirebaseAuth auth;
    private bool isAuthInit = false;
    private const string GOOGLE_WEB_CLIENT_ID = "222061272404-27lp0ocv653h3jci5vitlp1qq97otq3p.apps.googleusercontent.com";
    private GoogleSignInConfiguration googleSignInConfiguration;
    private FirebaseUser firebaseUser;
    
    // Firestore
    private const string UNITY_EDITOR_USER_ID = "9HyPrbDAf4Q1eLMhp9LVkxptBlx1";
    private FirebaseFirestore database;
    private bool isFirestoreInit = false;
    
    // Analytics
    private bool isAnalyticsInit = true;

    public bool HasSignedWithGoogle { get; private set; }
    public bool HasSignedWithApple { get; private set; }

    protected override void Init()
    {
        base.Init();
        
        LoadData();
        StartCoroutine(InitFirebaseServiceCoroutine());
    }

    public bool IsInit()
    {
        return isRemoteConfigInit && isAuthInit && isFirestoreInit && isAnalyticsInit;
    }

    private void LoadData()
    {
        HasSignedWithGoogle = PlayerPrefs.GetInt("HasSignedWithGoogle") == 1;
        HasSignedWithApple = PlayerPrefs.GetInt("HasSignedWithApple") == 1;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("HasSignedWithGoogle", HasSignedWithGoogle ? 1 : 0);
        PlayerPrefs.SetInt("HasSignedWithApple", HasSignedWithApple ? 1 : 0);
        PlayerPrefs.Save();
    }

    private IEnumerator InitFirebaseServiceCoroutine()
    {
        var checkTask = FirebaseApp.CheckAndFixDependenciesAsync();

        while (!checkTask.IsCompleted)
            yield return null;

        if (checkTask.IsFaulted || checkTask.IsCanceled)
        {
            Logger.LogError($"{GetType()}::FirebaseService could not be resolved.");
            yield break;
        }
        
        var dependencyStatus = checkTask.Result;

        if (dependencyStatus != DependencyStatus.Available)
        {
            Logger.LogError($"{GetType()}::FirebaseService dependency check failed.");
            yield break;
        }
        
        Logger.Log($"{GetType()}::FirebaseApp initialization success.");
        app = FirebaseApp.DefaultInstance;
        InitRemoteConfig();
        InitAuth();
        InitFirestore();
        InitAnalytics();

        var elapsedTime = 0.0f;
        while (elapsedTime < GlobalDefine.THIRD_PARTY_SERVICE_INIT_TIME)
        {
            if (IsInit())
            {
                Logger.Log($"{GetType()}:: initialization success.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Logger.LogError($"{GetType()}::FirebaseApp initialization failed.");
    }
    
    #region REMOTE_CONFIG

    private void InitRemoteConfig()
    {
        remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        if (remoteConfig == null)
        {
            Logger.LogError($"{GetType()}::FirebaseApp Initialization failed. FirebaseRemoteConfig is null.");
            return;
        }
        
        remoteConfigDic.Add("dev_app_version", string.Empty);
        remoteConfigDic.Add("real_app_version", string.Empty);
        remoteConfigDic.Add("openai_apikey", string.Empty);
        
        remoteConfig.SetDefaultsAsync(remoteConfigDic).ContinueWithOnMainThread(task =>
        {
            remoteConfig.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(fetchTask =>
            {
                if (fetchTask.IsCompleted)
                {
                    remoteConfig.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                    {
                        if (activateTask.IsCompleted)
                        {
                            remoteConfigDic["dev_app_version"] = remoteConfig.GetValue("dev_app_version").StringValue;
                            remoteConfigDic["real_app_version"] = remoteConfig.GetValue("real_app_version").StringValue;
                            remoteConfigDic["openai_apikey"] = remoteConfig.GetValue("openai_apikey").StringValue;
                            isRemoteConfigInit = true;
                        }
                    });
                }
            });
        });
    }

    public string GetAppVersion()
    {
#if DEV_VER
        if (remoteConfigDic.TryGetValue("dev_app_version", out var value))
        {
            return value.ToString();
        }
#else
        if (remoteConfigDic.TryGetValue("real_app_version", out var value))
        {
            return value.ToString();
        }
#endif
        return string.Empty;
    }

    public string GetOpenAIKey()
    {
        if (remoteConfigDic.TryGetValue("openai_apikey", out var value))
        {
            return value.ToString();
        }

        return string.Empty;
    }
    
    #endregion
    
    #region AUTH

    private void InitAuth()
    {
        auth = FirebaseAuth.DefaultInstance;
        if (auth == null)
        {
            Logger.LogError($"{GetType()}::FirebaseApp Initialization failed. FirebaseAuth is null.");
            return;
        }

        auth.StateChanged += OnAuthStateChanged;
        googleSignInConfiguration = new GoogleSignInConfiguration()
        {
            WebClientId = GOOGLE_WEB_CLIENT_ID,
            RequestIdToken = true
        };
        
        isAuthInit = true;

        if (auth.CurrentUser == null)
        {
            if (HasSignedWithGoogle)
            {
                SignInWithGoogle();
            }
            else if (HasSignedWithApple)
            {
                SignInWithApple();
            }
        }
        else
        {
            firebaseUser = auth.CurrentUser;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (SceneLoader.Instance.GetCurrentScene() == SceneType.Title)
        {
            return;
        }

        if (auth != null && auth.CurrentUser == null)
        {
            Logger.Log($"{GetType()}::User Signed out or disconnected");
            firebaseUser = null;
            HasSignedWithGoogle = false;
            HasSignedWithApple = false;
            SaveData();
            UIManager.Instance.CloseAllOpenUI();
            SceneLoader.Instance.LoadScene(SceneType.Title);
        }
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
        GoogleSignIn.Configuration = googleSignInConfiguration;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.IsCanceled)
                {
                    Logger.LogError($"{GetType()}::SignInWithGoogle was Canceled");
                }
                else if (task.IsFaulted)
                {
                    Logger.LogError($"{GetType()}::SignInWithGoogle was Faulted: {task.Exception}");
                }
                
                ShowLoginFailUI();
                return;
            }

            GoogleSignInUser googleUser = task.Result;
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    if (task.IsCanceled)
                    {
                        Logger.LogError($"{GetType()}::SignInWithGoogle was Canceled");
                    }
                    else if (task.IsFaulted)
                    {
                        Logger.LogError($"{GetType()}::SignInWithGoogle was Faulted: {task.Exception}");
                    }

                    ShowLoginFailUI();
                    return;
                }
                
                firebaseUser = task.Result;
                Logger.Log($"{GetType()}::User signed in successfully. {firebaseUser.DisplayName} ({firebaseUser.UserId})");

                HasSignedWithGoogle = true;
                HasSignedWithApple = true;
                SaveData();
            });
        });
    }

    public void SignInWithApple()
    {
        
    }

    public void SignOut()
    {
        if (firebaseUser != null)
        {
            auth.SignOut();
            Logger.Log($"{GetType()}::User signed out successfully.");
        }
        
#if UNITY_EDITOR
        Logger.Log($"{GetType()}::User Signed out or disconnected");
        firebaseUser = null;
        HasSignedWithGoogle = false;
        HasSignedWithApple = false;
        SaveData();
        UIManager.Instance.CloseAllOpenUI();
        SceneLoader.Instance.LoadScene(SceneType.Title);
#endif
    }

    private void ShowLoginFailUI()
    {
        var modal = new ModalUIData();
        modal.Type = ModalType.OK;
        modal.Title = "오류";
        modal.Desc = "로그인 실패";
        modal.OkBtnText = "확인";
        modal.OKAction = () =>
        {
            var modal = new ModalUIData();
            UIManager.Instance.OpenUI<AccountUI>(modal);
        };
        UIManager.Instance.OpenUI<ModalUI>(modal);
    }
    
    private string GetUserId()
    {
#if UNITY_EDITOR
        return UNITY_EDITOR_USER_ID;
#else
        return firebaseUser != null ? firebaseUser.UserId : string.Empty;
#endif
    }
    
    #endregion
    
    #region FIRESTORE
    private void InitFirestore()
    {
        database = FirebaseFirestore.DefaultInstance;
        if (database == null)
        {
            Logger.LogError($"FirebaseFirestore initialization failed. FirebaseFirestore is null");
            return;
        }
        
        isFirestoreInit = true;
    }

    public void LoadUserData<T>(Action onFinishLoad = null) where T : class, IUserData
    {
        Type type = typeof(T);
        database.Collection($"{type}").Document(GetUserId()).GetSnapshotAsync().ContinueWithOnMainThread<DocumentSnapshot>(task =>
        {
            if (task.IsCompleted)
            {
                IUserData userData = UserDataManager.Instance.GetUserData<T>();
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Logger.Log($"{GetType()}::{type} Loaded Successfully");
                    
                    Dictionary<string, object> userDataDict = snapshot.ToDictionary();
                    userData.Setting(userDataDict);
                }
                else
                {
                    Logger.Log($"{GetType()}::{type} No Found. setting default data");

                    userData.Initialize();
                    userData.SaveData();
                }
                
                onFinishLoad?.Invoke();
            }
            else
            {
                Logger.LogError($"{GetType()}::{type} Loading Failed");
            }
        });
    }

    public void SaveUserData<T>(Dictionary<string, object> userDataDict) where T : class, IUserData
    {
        Type type = typeof(T);
        DocumentReference documentReference = database.Collection($"{type}").Document(GetUserId());
        documentReference.SetAsync(userDataDict).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Logger.Log($"{type} saved Successfully");
            }
            else
            {
                Logger.LogError($"{type} saving Failed");
            }
        });
    }
    #endregion
    
    #region ANALYTICS

    private void InitAnalytics()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isAnalyticsInit = true;
    }

    public void LogCustomEvent(string eventName, Dictionary<string, object> parameters)
    {
        List<Parameter> firebaseParameters = new List<Parameter>();
        foreach (var param in parameters)
        {
            firebaseParameters.Add(new Parameter(param.Key, param.Value.ToString()));
        }
        
        FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());
    }
    #endregion

    protected override void Dispose()
    {
        base.Dispose();

        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }
}
