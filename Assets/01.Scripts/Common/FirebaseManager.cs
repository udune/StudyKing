using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
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
    private const string UNITY_EDITOR_USER_ID = "";
    private FirebaseFirestore database;
    private bool isFirestoreInit = false;

    protected override void Init()
    {
        base.Init();
        
        StartCoroutine(InitFirebaseServiceCoroutine());
    }

    public bool IsInit()
    {
        return isRemoteConfigInit && isAuthInit && isFirestoreInit;
    }

    private void SaveData(bool isSigned)
    {
        var userSignedData = UserDataManager.Instance.GetUserData<UserSignedData>();
        if (userSignedData == null)
        {
            Logger.Log($"{GetType()}::UserSignedData is null");
            return;
        }
        userSignedData.HasSignedWithGoogle = isSigned;
        userSignedData.HasSignedWithApple = isSigned;
        userSignedData.SaveData();
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
                InitFirestore();
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
            var userSignedData = UserDataManager.Instance.GetUserData<UserSignedData>();
            if (userSignedData == null)
            {
                Logger.Log($"{GetType()}::UserSignedData is null");
                return;
            }
            
            if (userSignedData.HasSignedWithGoogle)
            {
                SignInWithGoogle();
            }
            else if (userSignedData.HasSignedWithApple)
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
            SaveData(false);
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

                SaveData(true);
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
}
