using Logger = Common.Logger;

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    public LobbyController LobbyController { get; private set; }
    
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
}
