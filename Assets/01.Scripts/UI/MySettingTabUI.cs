
using System;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class MySettingTabUI : BaseUI
{
    [SerializeField] TMP_Text versionText;

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        versionText.text = $"버전 : {UnityEngine.Application.version}";
    }

    private void Awake()
    {
        versionText.text = $"버전 : {UnityEngine.Application.version}";
    }

    public void OnClickLogoutBtn()
    {
        Logger.Log($"{GetType()}::OnClickLogoutBtn");
        FirebaseManager.Instance.SignOut();
    }
}
