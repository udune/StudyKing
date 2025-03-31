using System.Collections;
using Common;
using Firebase;
using Firebase.Extensions;

public class FirebaseManager : SingletonBehaviour<FirebaseManager>
{
    private FirebaseApp app;

    protected override void Init()
    {
        base.Init();
        StartCoroutine(InitFirebaseServiceCoroutine());
    }

    private IEnumerator InitFirebaseServiceCoroutine()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Logger.Log($"FirebaseApp initialization success.");
                app = FirebaseApp.DefaultInstance;
            }
            else
            {
                Logger.LogError($"FirebaseApp initialization failed. Dependency : {dependencyStatus}");
            }
        });

        yield break;
    }
}
