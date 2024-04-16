﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redcode.Awaiting;
using SunsetSystems.Core.SceneLoading.UI;
using SunsetSystems.Persistence;
using SunsetSystems.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SunsetSystems.Core.SceneLoading
{
    public class LevelLoader : Singleton<LevelLoader>
    {
        [SerializeField]
        private SceneLoadingUIManager loadingScreenUI;
        [SerializeField]
        private float loadingCrossfadeTime = 1f;
        [SerializeField]
        private Camera loadingCamera;

        public SceneLoadingDataAsset.LevelLoadingData CurrentLoadedLevel { get; private set; }

        public static event Action OnLevelLoadStart, OnLevelLoadEnd, OnBeforePersistentDataLoad;
        public static event Action OnAfterScreenFadeOut, OnBeforeScreenFadeIn;

        public async Task LoadNewScene(SceneLoadingDataAsset data)
        {
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            await new WaitForUpdate();
            loadingCamera.gameObject.SetActive(true);
            SaveLoadManager.UpdateRuntimeDataCache();
            OnLevelLoadStart?.Invoke();
            loadingScreenUI.EnableAndResetLoadingScreen();
            await new WaitForSeconds(.5f);
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
            await DoSceneLoading(data.LoadingData);
            CurrentLoadedLevel = data.LoadingData;
            await new WaitForUpdate();
            OnBeforePersistentDataLoad?.Invoke();
            SaveLoadManager.InjectRuntimeDataIntoSaveables();
            await new WaitForSeconds(0.1f);
            OnLevelLoadEnd?.Invoke();
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            loadingCamera.gameObject.SetActive(false);
            loadingScreenUI.DisableLoadingScreen();
            await new WaitForSeconds(.1f);
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
        }

        public async Task LoadSavedGame(string saveID)
        {
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            await new WaitForUpdate();
            loadingCamera.gameObject.SetActive(true);
            OnLevelLoadStart?.Invoke();
            loadingScreenUI.EnableAndResetLoadingScreen();
            await new WaitForSeconds(.5f);
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
            var saveMetaData = SaveLoadManager.GetSaveMetaData(saveID);
            await DoSceneLoading(saveMetaData.LevelLoadingData);
            CurrentLoadedLevel = saveMetaData.LevelLoadingData;
            await new WaitForUpdate();
            OnBeforePersistentDataLoad?.Invoke();
            SaveLoadManager.LoadSavedDataIntoRuntime(saveID);
            SaveLoadManager.InjectRuntimeDataIntoSaveables();
            await new WaitForSeconds(0.1f);
            OnLevelLoadEnd?.Invoke();
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            loadingCamera.gameObject.SetActive(false);
            loadingScreenUI.DisableLoadingScreen();
            await new WaitForSeconds(.1f);
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        }

        private async Task DoSceneLoading(SceneLoadingDataAsset.LevelLoadingData data)
        {
            var asyncOp = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(data.AddressableScenePaths[0], LoadSceneMode.Single);
            while (asyncOp.IsDone == false)
            {
                loadingScreenUI.UpadteLoadingBar(asyncOp.PercentComplete);
                await Task.Yield();
            }

            List<Task> loadingOps = new();
            for (int i = 1; i < data.AddressableScenePaths.Count; i++)
            {
                loadingOps.Add(UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(data.AddressableScenePaths[i], LoadSceneMode.Additive).Task);
            }
            await Task.WhenAll(loadingOps);
        }
    }
}