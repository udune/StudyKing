using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : BaseUI
{
    public void OnClickResume()
    {
        LobbyManager.Instance.Resume();
        CloseUI();
    }

    public void OnClickQuit()
    {
        var data = new ModalUIData();
        data.Type = ModalType.OK_CANCEL;
        data.Title = "정말 나가시겠어요?";
        data.Desc = "지금까지 기록한 시간은 사라져요.";
        data.OkBtnText = "종료";
        data.CancelBtnText = "다시 시작";
        data.OKAction = () =>
        {
            LobbyManager.Instance.IsComplete = false;
            
            CloseUI();
            UIManager.Instance.GetActiveUI<StudyingUI>()?.CloseUI();
        };
        
        UIManager.Instance.OpenUI<ModalUI>(data);
    }
}
