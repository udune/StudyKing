using System;
using System.Collections.Generic;

namespace Common
{
    public interface IFirebaseManager
    {
        bool IsInit();
        bool IsSignedIn();
        void SignInWithGoogle();
        void SignInWithApple();
        void SignOut();
        string GetAppVersion();
        string GetOpenAIKey();
        void LoadUserData<T>(Action onFinishLoad = null) where T : class, IUserData;
        void SaveUserData<T>(Dictionary<string, object> userDataDict) where T : class, IUserData;
        void LogCustomEvent(string eventName, Dictionary<string, object> parameters);
    }
}