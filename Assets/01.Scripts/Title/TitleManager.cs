using System.Collections;
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
        public TextMeshProUGUI loadingText;
    
        private AsyncOperation async;

        private void Awake()
        {
            logoGo.SetActive(true);
            titleGo.SetActive(false);
            
            logoAnim = logoGo.GetComponent<Animation>();
        }

        private void Start()
        {
            UserDataManager.Instance.LoadUserData();

            if (!UserDataManager.Instance.IsExistSaveData)
            {
                UserDataManager.Instance.InitializeUserData();
                UserDataManager.Instance.SaveUserData();
            }
            
            UIManager.Instance.EnableTimeUI(false);
            UIManager.Instance.EnableTabUI(false);
            
            StartCoroutine(LoadingCo());
        }

        private IEnumerator LoadingCo()
        {
            Logger.Log($"{GetType()}::LoadCoroutine");
            logoAnim.Play();
            yield return new WaitForSeconds(logoAnim.clip.length);
        
            logoAnim.gameObject.SetActive(false);
            titleGo.SetActive(true);

            if (!CheckThirdPartyServiceInit())
            {
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

            yield return StartCoroutine(LoadLobbyCoroutine());
        }

        private bool CheckThirdPartyServiceInit()
        {
            return FirebaseManager.Instance.IsInit();
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
                modal.Type = ModalType.OK_CANCEL;
                modal.Title = string.Empty;
                modal.Desc = "앱 버전이 오래되었어요.<br>업데이트하시겠어요?";
                modal.OkBtnText = "업데이트";
                modal.CancelBtnText = "취소";
                modal.OKAction = () =>
                {
                    #if UNITY_ANDROID
                        Application.OpenURL(GlobalDefine.GOOGLE_PLAY_STORE);
                    #elif UNITY_IOS
                        Application.OpenURL(GlobalDefine.APPLE_PLAY_STORE);
                    #endif
                };
                modal.CANCELAction = () =>
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
            loadingText.text = ((int)loadingSlider.value * 100).ToString();
            yield return new WaitForSeconds(0.5f);

            while (!async.isDone)
            {
                loadingSlider.value = async.progress < 0.5f ? 0.5f : async.progress;
                loadingText.text = $"{(int)(loadingSlider.value * 100)}%";

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
