using Logger = Common.Logger;

public class AccountUI : BaseUI
{
    public void OnClickLogin()
    {
        Logger.Log($"{GetType()}::OnClickLogin");
        
        FirebaseManager.Instance.SignInWithGoogle();
        CloseUI();
    }

    public void OnClickAppleLogin()
    {
        Logger.Log($"{GetType()}::OnClickAppleLogin");
        
        FirebaseManager.Instance.SignInWithApple();
        CloseUI();
    }
}
