using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Audio;
using SunsetSystems.Blackboard;
using SunsetSystems.Core.SceneLoading;
using SunsetSystems.Journal;
using UnityEngine;

namespace SunsetSystems.Persistence
{
    public class SaveLoadManager : SerializedMonoBehaviour
    {
        [SerializeField]
        private Camera _screenShotCamera;

        private static SaveLoadManager _instance;

        private const string SAVE_PATH = "Saves/";
        private const string META_DATA = "SAVE_META";
        private const string GAME_DATA = "SAVE_GAME";
        private const string BLACKBOARD_DATA = "GLOBAL_BLACKBOARD";

        [ShowInInspector, ReadOnly]
        private GlobalPersistenceData _gameData = new();
        private static GlobalPersistenceData GameData => _instance._gameData;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeSaveSystem()
        {
            if (_instance != null)
                return;
            GameObject saveLoadGO = new("Save System Manager");
            DontDestroyOnLoad(saveLoadGO);
            _instance = saveLoadGO.AddComponent<SaveLoadManager>();
        }

        public static void ForceCreateNewSaveData()
        {
            _instance._gameData = new();
        }

        public static void CreateNewSaveFile(string saveName)
        {
            string date = $"{DateTime.Now:yyyy-M-dd--HH-mm-ss}";
            string saveID = Guid.NewGuid().ToString();
            string filename = SaveIDToFilePath(saveID);
            SaveMetaData metaData = new()
            {
                SaveName = saveName,
                SaveID = saveID,
                SaveDate = date,
                ActiveQuestName = QuestJournal.Instance.MainQuests.FirstOrDefault().Name,
                LevelLoadingData = LevelLoader.Instance.CurrentLoadedLevel,
                PlaylistData = AudioManager.Instance.GetCurrentPlaylistOverrides(),
                SaveScreenShot = TakeGameScreenShot(),
            };
            UpdateRuntimeDataCache();
            ES3.Save(META_DATA, metaData, filename);
            ES3.Save(GAME_DATA, GameData, filename);
            ES3.Save(BLACKBOARD_DATA, ScriptableVariableBase.GetSaveData(), filename);
        }

        public static void UpdateRuntimeDataCache()
        {
            foreach (ISaveable saveable in ISaveable.Saveables)
            {
                GameData.UpdateSaveData(saveable);
            }
        }

        public static void LoadSavedDataIntoRuntime(string saveID)
        {
            //_gameData.ClearSaveData();
            string filePath = SaveIDToFilePath(saveID);
            ES3.LoadInto(GAME_DATA, filePath, GameData);
            var blackboardData = ES3.Load<BlackboardSaveData>(BLACKBOARD_DATA, filePath);
            ScriptableVariableBase.InjectSaveData(blackboardData);
        }

        private static Texture2D TakeGameScreenShot()
        {
            var camera = _instance._screenShotCamera;
            if (camera != null)
            {
                Texture2D result = new(192, 108, TextureFormat.RGB24, false);
                RenderTexture renderTexture = new(192, 108, 24);
                var cameraTransform = camera.transform;
                cameraTransform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
                camera.gameObject.SetActive(true);
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                result.ReadPixels(new(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                result.Apply();
                camera.targetTexture = null;
                RenderTexture.active = null;
                Destroy(renderTexture);
                camera.gameObject.SetActive(false);
                return result;
            }
            return null;
        }

        public static IEnumerable<SaveMetaData> GetAllSaveMetaData()
        {
            List<SaveMetaData> result = new();
            string[] saveFiles = null;
            if (ES3.DirectoryExists(SAVE_PATH))
                saveFiles = ES3.GetFiles(SAVE_PATH);
            if (saveFiles == null)
                return result;
            foreach (var saveFile in saveFiles)
            {
                try
                {
                    result.Add(ES3.Load<SaveMetaData>(META_DATA, SAVE_PATH + saveFile));
                }
                catch (FormatException exception)
                {
                    UnityEngine.Debug.LogError($"Exception occured while loading a save file! File {saveFile} may be corrupted!");
                    UnityEngine.Debug.LogException(exception);
                    continue;
                }
            }
            return result;
        }

        public static SaveMetaData GetSaveMetaData(string saveID)
        {
            var metaData = ES3.Load<SaveMetaData>(META_DATA, SaveIDToFilePath(saveID));
            return metaData;
        }

        public static void InjectRuntimeDataIntoSaveables()
        {
            foreach (ISaveable saveable in ISaveable.Saveables)
            {
                Debug.Log($"SaveLoadManager >>> Started data injection into {saveable}!");
                if (GameData.TryGetData(saveable.DataKey, out object data))
                {
                    try
                    {
                        bool result = saveable.InjectSaveData(data);
                        if (result)
                        {
                            UnityEngine.Debug.Log($"SaveLoadManager >>> Data injection into {saveable} successful!");
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"SaveLoadManager >>> Data injection into {saveable} has failed!");
                        }
                    }
                    catch (Exception exception)
                    {
                        UnityEngine.Debug.LogException(exception, saveable as UnityEngine.Object);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"SaveLoadManager >>> There is no saved data for object {saveable}!");
                }
            }
        }

        public static LevelLoadingData GetSavedLevelAsset(string saveID)
        {
            var metaData = ES3.Load<SaveMetaData>(META_DATA, SaveIDToFilePath(saveID));
            if (metaData.SaveID == saveID)
                return metaData.LevelLoadingData;
            return new();
        }

        public static void DeleteSaveFile(string saveID)
        {
            string saveFileName = SaveIDToFilePath(saveID);
            ES3.DeleteFile(saveFileName);
        }

        private static string SaveIDToFilePath(string saveID)
        {
            return $"{SAVE_PATH}{saveID}.sav";
        }

        private static string SaveFileNameToSaveID(string fileName)
        {
            return fileName.Split('.')[0];
        }

        public static bool HasExistingSaves()
        {
            bool result = false;
            if (ES3.DirectoryExists(SAVE_PATH))
            {
                result = ES3.GetFiles(SAVE_PATH).Length > 0;
            }
            return result;
        }
    }
}
