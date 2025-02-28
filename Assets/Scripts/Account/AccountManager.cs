using UnityEngine;

namespace Account
{
    public class AccountManager : MonoBehaviour
    {
        private void Awake()
        {
            SceneLoader.Instance.LoadScene(SceneType.Lobby);
        }
    }
}
