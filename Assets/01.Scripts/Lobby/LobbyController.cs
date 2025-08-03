using UnityEngine;
using Logger = Common.Logger;

public class LobbyController : MonoBehaviour
{
    public void Init()
    {
        UIManager.Instance.SetTimeUIVisible(true);
        UIManager.Instance.SetTabUIVisible(true);
    }

    public void OnClickStudyBtn()
    {
        Logger.Log($"{GetType()}::OnClickStudyBtn");

        var data = new BaseUIData();
        UIManager.Instance.OpenUI<StudyUI>(data);
    }
}
