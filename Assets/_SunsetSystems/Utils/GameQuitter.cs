using SunsetSystems.Core.SceneLoading;
using UnityEngine;

namespace SunsetSystems.Utils
{
    public class GameQuitter : MonoBehaviour
    {
        public void QuitApplication()
        {
            Application.Quit();
        }

        public void LoadMainMenu()
        {
            LevelLoader.Instance.BackToMainMenu();
        }
    }
}
