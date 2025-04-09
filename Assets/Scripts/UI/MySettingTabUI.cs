
using Common;

public class MySettingTabUI : BaseUI
{
    public void OnClickLogoutBtn()
    {
        Logger.Log($"{GetType()}::OnClickLogoutBtn");
        FirebaseManager.Instance.SignOut();
    }
}
