using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redcode.Awaiting;
using Sirenix.OdinInspector;
using SunsetSystems.Audio;
using SunsetSystems.Core.SceneLoading.UI;
using SunsetSystems.Input.CameraControl;
using SunsetSystems.Party;
using SunsetSystems.Persistence;
using SunsetSystems.Utils;
using UMA;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SunsetSystems.Core.SceneLoading
{
    public class LevelLoader : Singleton<LevelLoader>
    {
        [SerializeField, Required]
        private SceneLoadingDataAsset _worldMapScene;
        [SerializeField]
        private SceneLoadingUIManager loadingScreenUI;
        [SerializeField]
        private float loadingCrossfadeTime = 1f;
        [SerializeField]
        private Camera loadingCamera;

        public LevelLoadingData CurrentLoadedLevel { get; private set; }

        public static event Action OnLevelLoadStart, OnLevelLoadEnd, OnBeforePersistentDataLoad, OnBeforePersistentDataCache;
        public static event Action OnAfterScreenFadeOut, OnBeforeScreenFadeIn;

        public async Task LoadWorldMap()
        {
            await LoadNewScene(_worldMapScene);
        }

        public async Task LoadNewScene(SceneLoadingDataAsset data)
        {
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            await new WaitForUpdate();
            OnAfterScreenFadeOut?.Invoke();
            loadingCamera.gameObject.SetActive(true);
            OnBeforePersistentDataCache?.Invoke();
            SaveLoadManager.UpdateRuntimeDataCache();
            OnLevelLoadStart?.Invoke();
            loadingScreenUI.EnableAndResetLoadingScreen();
            await new WaitForUpdate();
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
            await DoSceneLoading(data.LoadingData);
            CurrentLoadedLevel = data.LoadingData;
            OnBeforePersistentDataLoad?.Invoke();
            SaveLoadManager.InjectRuntimeDataIntoSaveables();
            await new WaitForUpdate();
            CameraControlScript.Instance.ForceToPosition(WaypointManager.Instance.GetSceneDefaultEntryWaypoint().transform);
            OnLevelLoadEnd?.Invoke();
            await new WaitUntil(() => HasGeneratorProcessedAllUMA() && PartyManager.Instance.IsInitialized);
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            loadingCamera.gameObject.SetActive(false);
            loadingScreenUI.DisableLoadingScreen();
            await new WaitForSeconds(.1f);
            OnBeforeScreenFadeIn?.Invoke();
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
        }

        private bool HasGeneratorProcessedAllUMA()
        {
            if (UMAGenerator.Instance != null)
                return UMAGenerator.Instance.IsIdle();
            return false;
        }

        public async Task LoadSavedGame(string saveID)
        {
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            await new WaitForUpdate();
            OnAfterScreenFadeOut?.Invoke();
            loadingCamera.gameObject.SetActive(true);
            OnLevelLoadStart?.Invoke();
            loadingScreenUI.EnableAndResetLoadingScreen();
            await new WaitForSeconds(.5f);
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
            var saveMetaData = SaveLoadManager.GetSaveMetaData(saveID);
            await DoSceneLoading(saveMetaData.LevelLoadingData);
            CurrentLoadedLevel = saveMetaData.LevelLoadingData;
            OnBeforePersistentDataLoad?.Invoke();
            SaveLoadManager.LoadSavedDataIntoRuntime(saveID);
            SaveLoadManager.InjectRuntimeDataIntoSaveables();
            await new WaitForSeconds(1f);
            AudioManager.Instance.InjectPlaylistDataAsOverrides(saveMetaData.PlaylistData);
            OnLevelLoadEnd?.Invoke();
            await new WaitUntil(() => HasGeneratorProcessedAllUMA() && PartyManager.Instance.IsInitialized);
            await loadingScreenUI.DoFadeOutAsync(loadingCrossfadeTime / 2f);
            loadingCamera.gameObject.SetActive(false);
            loadingScreenUI.DisableLoadingScreen();
            await new WaitForSeconds(.1f);
            OnBeforeScreenFadeIn?.Invoke();
            await loadingScreenUI.DoFadeInAsync(loadingCrossfadeTime / 2f);
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        }

        private async Task DoSceneLoading(LevelLoadingData data)
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