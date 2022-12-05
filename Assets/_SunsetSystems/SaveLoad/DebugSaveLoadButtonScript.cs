using SunsetSystems.Data;
using UnityEngine;
using SunsetSystems.Loading;
using System;
using SunsetSystems.Game;
using SunsetSystems.UI;

namespace SunsetSystems.Loading
{
    public class DebugSaveLoadButtonScript : MonoBehaviour
    {
        [SerializeField]
        private LevelLoader _sceneLoader;

        private void Start()
        {
            if (!_sceneLoader)
                _sceneLoader = FindObjectOfType<LevelLoader>();
        }

        public void DoSave()
        {
            Debug.Log("DoSave button");
            SaveLoadManager.Save();
        }

        public async void DoLoad()
        {
            Debug.Log("DoLoad button");
            PauseMenuUI menu = GetComponentInParent<PauseMenuUI>();
            Action action = null;
            if (GameManager.IsCurrentState(GameState.GamePaused))
                menu.gameObject.SetActive(false);
            else if (GameManager.IsCurrentState(GameState.Menu))
                action = FindObjectOfType<GameStarter>().DisableMainMenu;
            await _sceneLoader.LoadSavedLevel(action);
        }

        public void EnableLoading()
        {
            Debug.Log("ActionTest");
        }
    }
}
