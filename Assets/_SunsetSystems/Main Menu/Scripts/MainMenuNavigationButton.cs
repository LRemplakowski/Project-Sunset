using SunsetSystems.Core.SceneLoading.UI;
using UnityEngine;

namespace SunsetSystems.MainMenu.UI
{
    public class MainMenuNavigationButton : MonoBehaviour
    {
        [SerializeField]
        protected FadeScreenAnimator fadeUI;
        [SerializeField]
        protected GameObject currentGUIScreen;
        [SerializeField]
        protected GameObject targetGUIScreen;

        protected virtual void Start()
        {
            if (fadeUI == null) 
                fadeUI = FindAnyObjectByType<FadeScreenAnimator>(FindObjectsInactive.Include);
        }

        public async virtual void OnClick()
        {
            if (fadeUI == null) 
                fadeUI = FindAnyObjectByType<FadeScreenAnimator>(FindObjectsInactive.Include);
            await fadeUI.FadeOut(.5f);
            DoLoadTargetGUIScreen();
            await fadeUI.FadeIn(.5f);
        }

        protected void DoLoadTargetGUIScreen()
        {
            if (targetGUIScreen)
                targetGUIScreen.SetActive(true);
            currentGUIScreen.SetActive(false);
        }
    }
}
