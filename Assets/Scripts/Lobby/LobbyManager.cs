using System;
using Logger = Common.Logger;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    public LobbyController LobbyController { get; private set; }
    public bool IsPaused { get; set; }
    public bool IsComplete { get; set; }
    
    protected override void Init()
    {
        isDestroyOnLoad = true;
        base.Init();
    }

    private void Start()
    {
        LobbyController = FindObjectOfType<LobbyController>();
        if (LobbyController == null)
        {
            Logger.Log("LobbyController does not exist");
            return;
        }

        LobbyController.Init();
    }

    private void Update()
    {
        if (IsComplete)
        {
            return;
        }
        
        var checkCount = 0;
        var userStudyData = UserDataManager.Instance.GetUserData<UserStudyData>();
        if (userStudyData != null)
        {
            foreach (var itemData in userStudyData.StudyItemDataList)
            {
                if (itemData.Check)
                    checkCount++;
            }

            IsComplete = checkCount == userStudyData.StudyItemDataList.Count;
        }
    }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        IsPaused = false;
    }
}
