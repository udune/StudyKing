using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
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
        return isRemoteConfigInit && isAuthInit;
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
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Logger.Log($"{GetType()}::FirebaseApp initialization success.");
                app = FirebaseApp.DefaultInstance;
                InitRemoteConfig();

                InitAuth();
            }
            else
            {
                Logger.LogError($"{GetType()}::FirebaseApp initialization failed. Dependency : {dependencyStatus}");
            }
        });

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
    
    #endregion
}
