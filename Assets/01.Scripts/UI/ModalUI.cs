using System;
using TMPro;
using UnityEngine.UI;

public enum ModalType
{
    OK,
    OK_CANCEL
}

public class ModalUIData : BaseUIData
{
    public ModalType Type;
    public string Title;
    public string Desc;
    public string OkBtnText;
    public string CancelBtnText;
    public Action OKAction;
    public Action CANCELAction;
}

public class ModalUI : BaseUI
{
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI descText;
    public Button okBtn;
    public Button cancelBtn;
    public TextMeshProUGUI okBtnText;
    public TextMeshProUGUI cancelBtnText;

    private ModalUIData modalUIData;
    private Action OKAction;
    private Action CANCELAction;

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        
        modalUIData = data as ModalUIData;
        
        TitleText.text = modalUIData.Title;
        descText.text = modalUIData.Desc;
        okBtnText.text = modalUIData.OkBtnText;
        cancelBtnText.text = modalUIData.CancelBtnText;
        
        OKAction = modalUIData.OKAction;
        CANCELAction = modalUIData.CANCELAction;
        
        okBtn.gameObject.SetActive(true);
        cancelBtn.gameObject.SetActive(modalUIData.Type == ModalType.OK_CANCEL);
    }

    public void OnClickOKBtn()
    {
        OKAction?.Invoke();
        OKAction = null;
        CloseUI();
    }

    public void OnClickCancelBtn()
    {
        CANCELAction?.Invoke();
        CANCELAction = null;
        CloseUI();
    }
}
