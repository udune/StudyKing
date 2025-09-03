using System;
using TMPro;
using UnityEngine.UI;

public enum ModalType
{
    Ok,
    OkCancel
}

public class ModalUIData : BaseUIData
{
    public ModalType Type;
    public string Title;
    public string Desc;
    public string OkBtnText;
    public string CancelBtnText;
    public Action OkAction;
    public Action CancelAction;
}

public class ModalUI : BaseUI
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Button okBtn;
    public Button cancelBtn;
    public TextMeshProUGUI okBtnText;
    public TextMeshProUGUI cancelBtnText;

    private ModalUIData _modalUIData;
    private Action _okAction;
    private Action _cancelAction;

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        
        _modalUIData = data as ModalUIData;

        if (_modalUIData != null)
        {
            titleText.text = _modalUIData.Title;
            descText.text = _modalUIData.Desc;
            okBtnText.text = _modalUIData.OkBtnText;
            cancelBtnText.text = _modalUIData.CancelBtnText;

            _okAction = _modalUIData.OkAction;
            _cancelAction = _modalUIData.CancelAction;

            okBtn.gameObject.SetActive(true);
            cancelBtn.gameObject.SetActive(_modalUIData.Type == ModalType.OkCancel);
        }
    }

    public void OnClickOKBtn()
    {
        _okAction?.Invoke();
        _okAction = null;
        CloseUI();
    }

    public void OnClickCancelBtn()
    {
        _cancelAction?.Invoke();
        _cancelAction = null;
        CloseUI();
    }
}
