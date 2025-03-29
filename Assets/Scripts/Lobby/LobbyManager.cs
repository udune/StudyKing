using System;
using Logger = Common.Logger;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    public event Action OnCompleteChanged;
    
    public LobbyController LobbyController { get; private set; }
    public bool IsPaused { get; set; }

    private bool isComplete;

    public bool IsComplete
    {
        get => isComplete;
        set
        {
            if (isComplete.Equals(value))
                return;
            isComplete = value;
            OnCompleteChanged?.Invoke();
        }
    }
    
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

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        if (isComplete)
            return;
        
        IsPaused = false;
        var studyingUI = UIManager.Instance.GetActiveUI<StudyingUI>() as StudyingUI;
        studyingUI?.TimerStart();
    }
}
