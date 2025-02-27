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
            StartCoroutine(LoadingCo());
        }

        private IEnumerator LoadingCo()
        {
            Logger.Log($"{GetType()}::LoadCoroutine");
            logoAnim.Play();
            yield return new WaitForSeconds(logoAnim.clip.length);
        
            logoAnim.gameObject.SetActive(false);
            titleGo.SetActive(true);

            async = SceneLoader.Instance.LoadSceneAsync(SceneType.Account);
            if (async == null)
            {
                Logger.Log("Account async Loading Failed");
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
