using SunsetSystems.Data;
using UnityEngine;
using SunsetSystems.LevelManagement;
using System;
using SunsetSystems.Game;
using SunsetSystems.UI;

namespace SunsetSystems.LevelManagement
{
    public class DebugSaveLoadButtonScript : MonoBehaviour
    {
        [SerializeField]
        private LevelLoader _levelLoader;

        private void Start()
        {
            if (!_levelLoader)
                _levelLoader = LevelLoader.Instance;
        }

        public void DoSave()
        {
            Debug.Log("DoSave button");
            SaveLoadManager.SaveObjects();
        }

        public async void DoLoad()
        {
            Debug.Log("DoLoad button");
            await _levelLoader.LoadSavedLevel();
        }
    }
}
