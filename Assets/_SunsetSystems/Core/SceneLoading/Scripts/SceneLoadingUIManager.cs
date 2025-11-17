using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SunsetSystems.Core.SceneLoading.UI
{
    public class SceneLoadingUIManager : MonoBehaviour
    {
        [SerializeField]
        private LoadingScreenController loadingScreen;
        [SerializeField]
        private FadeScreenAnimator fadeScreen;
        [SerializeField]
        private CanvasGroup _canvasGroup;

        public static SceneLoadingUIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        private void Start()
        {
            if (loadingScreen == null)
                loadingScreen = GetComponentInChildren<LoadingScreenController>();
            if (fadeScreen == null)
                fadeScreen = GetComponentInChildren<FadeScreenAnimator>();
        }

        public async Task DoFadeOutAsync(float fadeOutTime)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            await fadeScreen.FadeOut(fadeOutTime);
        }

        public async Task DoFadeInAsync(float fadeInTime)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            await fadeScreen.FadeIn(fadeInTime);
        }

        public void UpadteLoadingBar(float value) => loadingScreen.SetLoadingProgress(value);

        public async UniTask EnableAndResetLoadingScreen()
        {
            loadingScreen.gameObject.SetActive(true);
            await loadingScreen.LoadRandomLoadingScreen();
        }

        public void DisableLoadingScreen()
        {
            loadingScreen.gameObject.SetActive(false);
        }
    }
}
