using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

namespace Title
{
    public class TitleManager : MonoBehaviour
    {
        private Animation logoAnim;
        
        [Header("LOGO")]
        public GameObject logoGo;
        
        [Space(10)]
        
        [Header("TITLE")]
        public GameObject titleGo;
        public Slider loadingSlider;
        // public TextMeshProUGUI loadingText;
    
        private AsyncOperation async;

        private void Awake()
        {
            logoGo.SetActive(true);
            titleGo.SetActive(false);
            
            logoAnim = logoGo.GetComponent<Animation>();
        }

        private void Start()
        {
            UIManager.Instance.SetTimeUIVisible(false);
            UIManager.Instance.SetTabUIVisible(false);
            
            StartCoroutine(LoadingCo());
        }

        private IEnumerator LoadingCo()
        {
            Logger.Log($"{GetType()}::LoadCoroutine");
            logoAnim.Play();
            yield return new WaitForSeconds(logoAnim.clip.length);
        
            logoAnim.gameObject.SetActive(false);
            titleGo.SetActive(true);

            yield return WaitForFirebaseInitAsync();
            
            if (!FirebaseManager.Instance.IsInit())
            {
                var modal = new ModalUIData();
                modal.Type = ModalType.Ok;
                modal.Title = "네트워크 오류";
                modal.Desc = "네트워크 초기화에 실패했습니다.\n앱을 종료합니다.";
                modal.OkBtnText = "다시 시도";
                modal.OkAction = Application.Quit;
                UIManager.Instance.OpenUI<ModalUI>(modal);
                
                yield break;
            }

            if (!ValidateAppVersion())
            {
                yield break;
            }

            if (!FirebaseManager.Instance.IsSignedIn())
            {
                var modal = new ModalUIData();
                UIManager.Instance.OpenUI<AccountUI>(modal);
            }

            while (!FirebaseManager.Instance.IsSignedIn())
            {
                yield return null;
            }

#if !UNITY_EDITOR
            UserDataManager.Instance.LoadUserData();

            while (!UserDataManager.Instance.IsUserDataLoaded())
            {
                yield return null;
            }
#endif
            yield return StartCoroutine(LoadLobbyCoroutine());
        }

        private async Task<bool> WaitForFirebaseInitAsync()
        {
            const float timeout = 10.0f;
            float elapsedTime = 0.0f;

            while (!FirebaseManager.Instance.IsInit() && elapsedTime < timeout)
            {
                await Task.Delay(100);
                elapsedTime += 0.1f;
            }

            if (elapsedTime >= timeout)
            {
                Logger.LogError($"{GetType()}::Firebase Init Timeout");
                return false;
            }

            return true;
        }

        private bool ValidateAppVersion()
        {
            bool result = false;
            if (Application.version == FirebaseManager.Instance.GetAppVersion())
            {
                result = true;
            }
            else
            {
                var modal = new ModalUIData();
                modal.Type = ModalType.OkCancel;
                modal.Title = string.Empty;
                modal.Desc = "앱 버전이 오래되었어요.<br>업데이트하시겠어요?";
                modal.OkBtnText = "업데이트";
                modal.CancelBtnText = "취소";
                modal.OkAction = () =>
                {
                    #if UNITY_ANDROID
                        Application.OpenURL(Constants.GlobalDefine.GOOGLE_PLAY_STORE);
                    #elif UNITY_IOS
                        Application.OpenURL(GlobalDefine.APPLE_PLAY_STORE);
                    #endif
                };
                modal.CancelAction = () =>
                {
                    Application.Quit();
                };
                UIManager.Instance.OpenUI<ModalUI>(modal);
            }
            
            return result;
        }
        
        private IEnumerator LoadLobbyCoroutine()
        {
            async = SceneLoader.Instance.LoadSceneAsync(SceneType.Lobby);
            if (async == null)
            {
                Logger.Log($"{GetType()}::Account async Loading Failed");
                yield break;
            }
            
            async.allowSceneActivation = false;

            loadingSlider.value = 0.5f;
            //loadingText.text = ((int)loadingSlider.value * 100).ToString();
            yield return new WaitForSeconds(0.5f);

            while (!async.isDone)
            {
                loadingSlider.value = async.progress < 0.5f ? 0.5f : async.progress;
                //loadingText.text = $"{(int)(loadingSlider.value * 100)}%";

                if (async.progress >= 0.9f)
                {
                    async.allowSceneActivation = true;
                    yield break;
                }

                yield return null;
            }
        }
    }
}
