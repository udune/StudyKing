using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;
using Logger = Common.Logger;

public class FirebaseManager : SingletonBehaviour<FirebaseManager>
{
    private FirebaseApp app;
    private FirebaseRemoteConfig remoteConfig;
    private bool isRemoteConfigInit;
    private Dictionary<string, object> remoteConfigDic = new Dictionary<string, object>();

    protected override void Init()
    {
        base.Init();
        StartCoroutine(InitFirebaseServiceCoroutine());
    }

    public bool IsInit()
    {
        return isRemoteConfigInit;
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
}
