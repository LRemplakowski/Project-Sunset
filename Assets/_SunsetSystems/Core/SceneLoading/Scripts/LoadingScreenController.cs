using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Core.SceneLoading.UI
{
    public class LoadingScreenController : SerializedMonoBehaviour
    {
        [Title("Runtime References")]
        [SerializeField]
        private Slider loadingBar;
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private TextMeshProUGUI loadingBarText;
        [SerializeField]
        private Button continueButton;
        [SerializeField]
        private FadeScreenAnimator transitionAnimator;
        [Title("Asset References")]
        [SerializeField]
        private LoadingScreenProviderConfig loadingScreenProviderConfig;

        private void OnEnable()
        {
            if (loadingBar == null)
                loadingBar = GetComponentInChildren<Slider>();
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            if (loadingBarText == null)
                loadingBarText = GetComponentInChildren<TextMeshProUGUI>();
            if (continueButton == null)
                continueButton = GetComponentInChildren<Button>();
            loadingBar.value = 0f;
            continueButton.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            loadingScreenProviderConfig.ReleaseLoadingScreens();           
        }

        public void SetUnloadingProgress(float value)
        {
            loadingBar.value = value / 2;
            //loadingBarText.text = value * 50f + " %";
        }

        public void SetLoadingProgress(float value)
        {
            loadingBar.value = (value / 2) + .5f;
            //loadingBarText.text = (value * 50f) + 50f + " %";
        }

        public void EnableContinue()
        {
            Debug.Log("enabling continue button");
            continueButton.gameObject.SetActive(true);
        }

        public async void OnContinue()
        {
            await transitionAnimator.FadeOut(.5f);
            DisableLoadingScreen();
            await transitionAnimator.FadeIn(.5f);
        }

        private void DisableLoadingScreen()
        {
            this.gameObject.SetActive(false);
        }

        [Button]
        public async UniTask LoadRandomLoadingScreen()
        {
            var screen = await loadingScreenProviderConfig.GetRandomLoadingScreenAsync();
            if (screen == null)
                Debug.LogError("Loading Screen Provider returned a null loading screen sprite!");
            backgroundImage.sprite = screen;
        }
    }
}
