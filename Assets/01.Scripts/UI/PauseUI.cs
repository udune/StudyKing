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
        data.Title = "정말 종료할까요?";
        data.Desc = "시간이 리셋됩니다.";
        data.OkBtnText = "종료";
        data.CancelBtnText = "취소";
        data.OKAction = () =>
        {
            LobbyManager.Instance.IsComplete = false;
            
            CloseUI();
            UIManager.Instance.GetActiveUI<StudyingUI>()?.CloseUI();
        };
        
        UIManager.Instance.OpenUI<ModalUI>(data);
    }
}
