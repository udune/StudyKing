
using System;
using TMPro;
using UnityEngine;
using Logger = Common.Logger;

public class MySettingTabUI : BaseUI
{
    [SerializeField] TMP_Text versionText;

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
