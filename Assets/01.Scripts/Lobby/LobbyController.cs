using UnityEngine;
using Logger = Common.Logger;

public class LobbyController : MonoBehaviour
{
    public void Init()
    {
        UIManager.Instance.EnableTimeUI(true);
        UIManager.Instance.EnableTabUI(true);
    }

    public void OnClickStudyBtn()
    {
        Logger.Log($"{GetType()}::OnClickStudyBtn");

        var data = new BaseUIData();
        UIManager.Instance.OpenUI<StudyUI>(data);
    }
}
